using UnityEngine.Networking;

namespace WebKit.Decorator
{
    public class AuthDecorator : RequestDecorator
    {
        private const string BEARER_KEY = "Bearer ";
        private const string AUTHORIZATION_KEY = "Authorization";
        private readonly string _authToken;

        public AuthDecorator(RequestDecorator? innerRequestDecorator, string token) : base(innerRequestDecorator)
        {
            _authToken = BEARER_KEY + token;
        }

        public override void Decorate(UnityWebRequest unityWebRequest)
        {
            base.Decorate(unityWebRequest);
            unityWebRequest.SetRequestHeader(AUTHORIZATION_KEY, _authToken);
        }
    }
}