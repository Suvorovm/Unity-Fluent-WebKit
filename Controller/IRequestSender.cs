using Cysharp.Threading.Tasks;
using WebKit.Decorator;
using WebKit.Model;

namespace WebKit.Controller
{
    public interface IRequestSender
    {
        void Init(string rootHost, RequestDecorator requestDecorator);

        UniTask<T> MakeRequest<T>(RequestModel requestModel, RequestDecorator? decorator = null)
            where T : new();

        UniTask MakeRequest(RequestModel requestModel, RequestDecorator? decorator = null);

        void SetDecorator(RequestDecorator requestDecorator);

        RequestDecorator? GetRequestDecorator();

        void AddRetryPolicy(IRetryPolicy policy);
    }
}