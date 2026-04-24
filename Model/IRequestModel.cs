using System.Collections.Generic;

namespace WebKit.Model
{
    public interface IRequestModel
    {
        RequestType GetRequestType();

        string GetPath();

        Dictionary<string, string> GetParams();

        object? GetBody();

        int GetTimeOut();

        int GetMaxRetryCount();
    }
}