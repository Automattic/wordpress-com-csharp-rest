using RestSharp;
using System;
using System.Linq;

namespace WordPress_csharp_REST_client
{
    class RestSharpDotComAuthenticator : IAuthenticator
    {

        public String AccessToken { get; set; }
        public static String REST_AUTHORIZATION_HEADER = "Authorization";
        public static String REST_AUTHORIZATION_FORMAT = "Bearer {0}";

        public RestSharpDotComAuthenticator(String oAuthToken)
        {
            this.AccessToken = oAuthToken;
        }

        void IAuthenticator.Authenticate(IRestClient client, IRestRequest request)
        {
            if (AccessToken == null)
            {
                //remove the token if already there for some reason ??
                if (request.Parameters.Any(p => p.Name.Equals(REST_AUTHORIZATION_HEADER, StringComparison.OrdinalIgnoreCase)))
                {
                    request.AddParameter(REST_AUTHORIZATION_HEADER, null, ParameterType.HttpHeader);
                }
            }
            else
            {
                // only add the Authorization parameter if it hasn't been added by a previous Execute
                if (!request.Parameters.Any(p => p.Name.Equals(REST_AUTHORIZATION_HEADER, StringComparison.OrdinalIgnoreCase)))
                {
                    //Add the Auth header
                    string authHeader = string.Format(REST_AUTHORIZATION_FORMAT, AccessToken);
                    request.AddParameter(REST_AUTHORIZATION_HEADER, authHeader, ParameterType.HttpHeader);
                }
            }
        }
    }
}
