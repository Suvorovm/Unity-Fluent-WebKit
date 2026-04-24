using UnityEngine.Networking;

namespace WebKit.Decorator
{
    public abstract class RequestDecorator
    {
        protected readonly RequestDecorator InnerRequestDecorator;

        protected RequestDecorator()
        {
        }

        protected RequestDecorator(RequestDecorator innerRequestDecorator)
        {
            InnerRequestDecorator = innerRequestDecorator;
        }

        public virtual void Decorate(UnityWebRequest unityWebRequest)
        {
            InnerRequestDecorator?.Decorate(unityWebRequest);
        }
    }
}