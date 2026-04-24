using System;
using System.Collections.Generic;
using System.Threading;
using CGK.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using WebKit.Decorator;
using WebKit.Error;
using WebKit.Model;

namespace WebKit.Controller
{
    public class UnityWebRequestSender : IRequestSender
    {
        private const int DEFAULT_MAX_RETRY_ATTEMPTS = 3;
        private RequestDecorator _requestDecorator = null!;
        private string _rootHost = null!;
        private readonly List<IRetryPolicy> _retryPolicies = new();

        public void Init(string rootHost, RequestDecorator requestDecorator)
        {
            _rootHost = rootHost;
            _requestDecorator = requestDecorator;
        }

        public void AddRetryPolicy(IRetryPolicy policy)
        {
            _retryPolicies.Add(policy);
        }

        public async UniTask<T> MakeRequest<T>(RequestModel requestModel, RequestDecorator? decorator = null)
            where T : new()
        {
            string json = await SendRequest(requestModel, decorator);
            T? response;
            try
            {
                if (json.IsNullOrEmpty())
                {
                    throw new WebRequestError("Result is null or empty");
                }

                response = JsonConvert.DeserializeObject<T>(json);
                if (response == null)
                {
                    throw new WebRequestError("Can't Deserialize");
                }
            }
            catch (Exception e)
            {
                throw new WebRequestError("Result is null or empty", e);
            }

            return response;
        }

        public async UniTask MakeRequest(RequestModel requestModel, RequestDecorator? decorator = null)
        {
            await SendRequest(requestModel, decorator);
        }

        public void SetDecorator(RequestDecorator requestDecorator)
        {
            _requestDecorator = requestDecorator;
        }
        public RequestDecorator? GetRequestDecorator()
        {
            return _requestDecorator;
        }

        private async UniTask<string> SendRequest(RequestModel requestModel, RequestDecorator? decorator)
        {
            int timeout = requestModel.GetTimeOut();
            int maxRetryCount = requestModel.GetMaxRetryCount() > 0
                ? requestModel.GetMaxRetryCount()
                : DEFAULT_MAX_RETRY_ATTEMPTS;
            for (int attempt = 0; attempt <= maxRetryCount; attempt++)
            {
                RequestDecorator? currentDecorator = attempt == 0 ? decorator : null;
                using UnityWebRequest request = CreateRequest(requestModel, currentDecorator);
                UnityWebRequest result = await SendOnce(request, timeout);

                if (result.result == UnityWebRequest.Result.Success)
                {
                    return result.downloadHandler.text;
                }

                string error = result.error;
                long responseCode = result.responseCode;

                IRetryPolicy? policy = FindMatchingPolicy(result);
                if (policy == null)
                {
                    throw new WebRequestError(error, responseCode);
                }

                bool prepared = await policy.PrepareRetryAsync();
                if (!prepared)
                {
                    throw new WebRequestError(error, responseCode);
                }
            }

            throw new WebRequestError("Max retry attempts exceeded", 0);
        }

        private IRetryPolicy? FindMatchingPolicy(UnityWebRequest request)
        {
            foreach (IRetryPolicy policy in _retryPolicies)
            {
                if (policy.ShouldHandle(request))
                    return policy;
            }

            return null;
        }

        private async UniTask<UnityWebRequest> SendOnce(UnityWebRequest request, int timeoutSeconds)
        {
            CancellationTokenSource token = new CancellationTokenSource();
            try
            {
                token.CancelAfterSlim(TimeSpan.FromSeconds(timeoutSeconds));
                return await request.SendWebRequest().WithCancellation(token.Token);
            }
            catch (UnityWebRequestException e)
            {
                return e.UnityWebRequest;
            }
            catch (OperationCanceledException)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Request timed out: {request.url}");
#endif
                request.Abort();
                return request;
            }
            finally
            {
                token.Cancel();
                token.Dispose();
            }
        }

        private UnityWebRequest CreateRequest(RequestModel requestModel, RequestDecorator? decorator = null)
        {
            string fullURL = GetFullURLWithParams(requestModel);

            UnityWebRequest request = new UnityWebRequest(fullURL, requestModel.GetRequestType().ToString().ToUpper());
            TryCreateBody(request, requestModel);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (decorator != null)
            {
                decorator.Decorate(request);
            }
            else
            {
                _requestDecorator?.Decorate(request);
            }

            return request;
        }

        private void TryCreateBody(UnityWebRequest request, RequestModel requestModel)
        {
            object? bodyObject = requestModel.GetBody();
            if (bodyObject == null)
            {
                return;
            }

            string bodyJson = JsonConvert.SerializeObject(bodyObject);
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(bodyJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        }

        private string GetFullURLWithParams(RequestModel requestModel)
        {
            string resultUrl = _rootHost + requestModel.GetPath();
            Dictionary<string, string> requestParams = requestModel.GetParams();
            if (requestParams.Count == 0)
            {
                return resultUrl;
            }

            bool firstParam = true;
            foreach ((string key, string value) in requestParams)
            {
                string splitCharacter = firstParam ? "?" : "&";
                if (firstParam)
                {
                    firstParam = false;
                }

                resultUrl += splitCharacter + $"{key}={Uri.EscapeDataString(value)}";
            }

            return resultUrl;
        }
    }
}