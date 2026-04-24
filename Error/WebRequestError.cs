using System;

namespace WebKit.Error
{
    public class WebRequestError : Exception
    {
        public long HttpStatusCode { get; }

        public WebRequestError(string webErrorMessage, long httpStatusCode = 0) : base(webErrorMessage)
        {
            HttpStatusCode = httpStatusCode;
        }

        public WebRequestError(string errorMessage, Exception innerException, long httpStatusCode = 0)
            : base(errorMessage, innerException)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}