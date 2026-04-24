using System.Collections.Generic;

namespace WebKit.Model
{
    public class RequestModel : IRequestModel
    {
        private string _path = null!;
        private int _timeOut = 30;
        private int _maxRetryCount = -1;
        private Dictionary<string, string> _params = new Dictionary<string, string>();
        private object? _body;
        private RequestType _requestType;
        
        public RequestType GetRequestType()
        {
            return _requestType;
        }

        public string GetPath()
        {
            return _path;
        }

        public Dictionary<string, string> GetParams()
        {
            return _params;
        }

        public object? GetBody()
        {
            return _body;
        }
        
        public int GetTimeOut()
        {
            return _timeOut;
        }
        public int GetMaxRetryCount()
        {
            return _maxRetryCount;
        }

        public void SetPath(string path)
        {
            _path = path;
        }

        public void SetMaxRetryCount(int maxRetryCount)
        {
            _maxRetryCount = maxRetryCount;
        }
        public void SetTimeOut(int timeOutInSeconds)
        {
            _timeOut = timeOutInSeconds;
        }

        public void AddParam(string key, string value)
        {
            _params.Add(key, value);
        }

        public void SetBody(object? body)
        {
            _body = body;
        }

        public void SetRequestType(RequestType requestType)
        {
            _requestType = requestType;
        }
    }
}