using System.Collections.Generic;

namespace WebKit.Model
{
    public class RequestBuilder
    {
        private RequestModel _requestModel;

        private RequestBuilder()
        {
            _requestModel = new RequestModel();
        }

        public static RequestBuilder Create()
        {
            return new RequestBuilder();
        }

        public RequestBuilder SetPath(string path)
        {
            _requestModel.SetPath(path);
            return this;
        }

        public RequestBuilder SetTimeOut(int timeOutInSeconds)
        {
            _requestModel.SetTimeOut(timeOutInSeconds);
            return this;
        }

        public RequestBuilder AddParam(string key, string value)
        {
            _requestModel.AddParam(key, value);
            return this;
        }
        
        public RequestBuilder AddParam(string key, object value)
        {
            _requestModel.AddParam(key, value.ToString());
            return this;
        }

        public RequestBuilder SetParams(Dictionary<string, string> parameters)
        {
            foreach ((string key, string value) in parameters)
            {
                _requestModel.AddParam(key, value);
            }

            return this;
        }

        public RequestBuilder SetBody(object requestBody)
        {
            _requestModel.SetBody(requestBody);
            return this;
        }

        public RequestBuilder SetMaxRetryCount(int retryCount)
        {
            _requestModel.SetMaxRetryCount(retryCount);
            return this;
        }

        public RequestBuilder SetRequestType(RequestType requestType)
        {
            _requestModel.SetRequestType(requestType);
            return this;
        }

        public RequestModel Build()
        {
            return _requestModel;
        }
    }
}