using UnityEngine.Networking;

namespace WebKit.Decorator
{
    public class HttpsRequestDecorator : RequestDecorator
    {
        private readonly CertificateHandler _certificateHandler;
        public HttpsRequestDecorator(CertificateHandler certificateHandler)
        {
            _certificateHandler = certificateHandler;
        }
        public override void Decorate(UnityWebRequest unityWebRequest)
        {
            base.Decorate(unityWebRequest);
            unityWebRequest.certificateHandler = _certificateHandler;
        }
    }
}