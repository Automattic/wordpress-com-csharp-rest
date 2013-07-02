using RestSharp;
using System;

namespace WordPress_csharp_REST_client
{
    public class OAuth
    {
        public static String AUTHORIZE_ENDPOINT = "https://public-api.wordpress.com/oauth2/authorize";
        private static String AUTHORIZED_ENDPOINT_FORMAT = "{0}?client_id={1}&redirect_uri={2}&response_type=code";

        public static String TOKEN_ENDPOINT = "https://public-api.wordpress.com/oauth2/token";

        private static String CLIENT_ID_PARAM_NAME = "client_id";
        private static String REDIRECT_URI_PARAM_NAME = "redirect_uri";
        private static String CLIENT_SECRET_PARAM_NAME = "client_secret";
        private static String CODE_PARAM_NAME = "code";
        private static String GRANT_TYPE_PARAM_NAME = "grant_type";
        private static String USERNAME_PARAM_NAME = "username";
        private static String PASSWORD_PARAM_NAME = "password";

        private static String PASSWORD_GRANT_TYPE = "password";
        private static String BEARER_GRANT_TYPE = "bearer";
        private static String AUTHORIZATION_CODE_GRANT_TYPE = "authorization_code";

        private String _appId;
        private String _appSecret;
        private String _appRedirectURI;

        public OAuth(String appId, String appSecret, String redirectURI)
        {
            _appId = appId;
            _appSecret = appSecret;
            _appRedirectURI = redirectURI;
        }

        public String getAuthorizationURL()
        {
            return String.Format(AUTHORIZED_ENDPOINT_FORMAT, AUTHORIZE_ENDPOINT, _appId, _appRedirectURI);
        }

        public RestRequest makePasswordRequest(String username, String password)
        {
            var request = new RestRequest();
            request.AddParameter(CLIENT_ID_PARAM_NAME, _appId, ParameterType.GetOrPost);
            request.AddParameter(REDIRECT_URI_PARAM_NAME, _appRedirectURI, ParameterType.GetOrPost);
            request.AddParameter(USERNAME_PARAM_NAME, username, ParameterType.GetOrPost);
            request.AddParameter(PASSWORD_PARAM_NAME, password, ParameterType.GetOrPost);
            request.AddParameter(GRANT_TYPE_PARAM_NAME, PASSWORD_GRANT_TYPE, ParameterType.GetOrPost);
            request.Method = RestSharp.Method.POST;
            
            return request;
        }

        public RestRequest makeBearerRequest(String code)
        {
            var request = new RestRequest();
            request.AddParameter(CLIENT_ID_PARAM_NAME, _appId, ParameterType.GetOrPost);
            request.AddParameter(REDIRECT_URI_PARAM_NAME, _appRedirectURI, ParameterType.GetOrPost);
            request.AddParameter(CODE_PARAM_NAME, code, ParameterType.GetOrPost);
            request.AddParameter(GRANT_TYPE_PARAM_NAME, BEARER_GRANT_TYPE, ParameterType.GetOrPost);
            request.Method = RestSharp.Method.POST;

            return request;
        }

    }


    public class Token
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string blog_url { get; set; }
        public string scope { get; set; }
        public string blog_id { get; set; }
    }
}