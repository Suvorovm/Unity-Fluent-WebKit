using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace WebKit.Controller
{
    public interface IRetryPolicy
    {
        bool ShouldHandle(UnityWebRequest failedRequest);
        UniTask<bool> PrepareRetryAsync();
    }
}
