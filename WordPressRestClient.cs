using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace WordPress_csharp_REST_client
{
    public class WordPressRestClient
    {

        private static String REST_API_ENDPOINT_URL = "https://public-api.wordpress.com/rest/v1/";

        private static String NOTIFICATION_FIELDS = "id,type,unread,body,subject,timestamp";
        private static String COMMENT_REPLY_CONTENT_FIELD = "content";

        /// <summary>
        ///  Socket timeout in milliseconds for GET rest requests
        /// </summary>
        public static int REST_TIMEOUT_MS = 30000;

        /// <summary>
        ///  Default number of retries for POST rest requests
        /// </summary>
        public static int REST_MAX_RETRIES_POST = 0;

        /// <summary>
        ///  Default number of retries for GET rest requests
        /// </summary>
        public static int REST_MAX_RETRIES_GET = 3;

        public static int REST_MAX_REDIRECTS = 3;

        /// <summary>
        ///  Socket timeout in milliseconds for rest requests
        /// </summary>
        public int Timeout
        {
            get
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                return _client.Timeout;
            }

            set
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                _client.Timeout = value;
            }
        }

        /// <summary>
        ///  The User-Agent header to be sent with each future request.
        /// </summary>
        public String UserAgent
        {
            get
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                return _client.UserAgent;
            }

            set
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                _client.UserAgent = value;
            }
        }

        public int? MaxRedirects
        {
            get
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                return _client.MaxRedirects;
            }

            set
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                _client.MaxRedirects = value;
            }
        }

        public bool FollowRedirects
        {
            get
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                return _client.FollowRedirects;
            }

            set
            {
                if (_client == null)
                    throw new Exception("Inner Rest Client cannot be null");
                _client.FollowRedirects = value;
            }
        }

        private RestClient _client;

        public IWordPressAuthenticator WordPressAuthenticator { get; set; }

        private String _accessToken = null;

        public String AccessToken
        {
            get
            {
                return _accessToken;
            }
            set
            {
                _accessToken = value;
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _client.Authenticator = new RestSharpDotComAuthenticator(value);
                }
                else
                {
                    _client.Authenticator = null;
                }
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return !String.IsNullOrWhiteSpace(_accessToken);
            }
        }


        /// <summary>
        ///  Convenience method that remove the current access token
        /// </summary>
        public void clearAccessToken()
        {
            this.AccessToken = null;
        }

        public WordPressRestClient()
        {
            _client = new RestClient();
            //defaults
            _client.Timeout = REST_TIMEOUT_MS;
            _client.FollowRedirects = true;
            _client.MaxRedirects = REST_MAX_REDIRECTS;
            _client.BaseUrl = REST_API_ENDPOINT_URL;

            // silverlight friendly way to get current version
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            var version = assemblyName.Version;
            _client.UserAgent = "wordpress-csharp/" + version;
        }

        public WordPressRestClient(String accessToken)
            : this()
        {
            this.AccessToken = accessToken;
        }

        public WordPressRestClient(IWordPressAuthenticator authenticator)
            : this()
        {
            this.WordPressAuthenticator = authenticator;
        }

        private void makeRequest<T>(RestSharp.Method method, String resourceURL, Dictionary<string, string> parameters, Action<IRestResponse<T>> callback) where T : new()
        {
            var request = new RestRequest();
            request.Method = method;
            request.Resource = resourceURL;
            _client.BaseUrl = REST_API_ENDPOINT_URL;

            if (parameters != null)
            {
               // Loop over pairs with foreach
                foreach (KeyValuePair<string, string> pair in parameters)
                {
                    request.AddParameter(pair.Key, pair.Value, ParameterType.GetOrPost);
                }
            }

            WordPressRestRequest wpRequest = new WordPressRestRequest(this, request);
            wpRequest.send(callback);
        }

        /// <summary>
        ///  Make GET request
        /// </summary>
        public void get<T>(String path, Action<IRestResponse<T>> callback) where T : new()
        {
            this.get(path, null, callback);
        }

        /// <summary>
        ///  Make GET request with params
        /// </summary>
        public void get<T>(String path, Dictionary<string, string> parameters, Action<IRestResponse<T>> callback) where T : new()
        {
            this.makeRequest(RestSharp.Method.GET, getResourceURL(path, parameters), null, callback);
        }

        /// <summary>
        /// Get notifications with default params.
        ///
        /// https://developer.wordpress.com/docs/api/1/get/notifications/
        /// </summary>
        public void getNotifications<T>(Action<IRestResponse<T>> callback) where T : new()
        {
            getNotifications(new Dictionary<String, String>(), callback);
        }

        /// <summary>
        /// Get notifications with provided params.
        ///
        /// https://developer.wordpress.com/docs/api/1/get/notifications/
        /// </summary>
        public void getNotifications<T>(Dictionary<String, String> parameters, Action<IRestResponse<T>> callback) where T : new()
        {
            parameters.Add("number", "40");
            parameters.Add("num_note_items", "20");
            parameters.Add("fields", NOTIFICATION_FIELDS);
            get("notifications", parameters, callback);
        }

        /// <summary>
        ///  Make POST request
        /// </summary>
        public void post<T>(String path, Action<IRestResponse<T>> callback) where T : new()
        {
            this.post(path, null, callback);
        }

        /// <summary>
        ///  Make POST request with params
        /// </summary>
        public void post<T>(String path, Dictionary<string, string> parameters, Action<IRestResponse<T>> callback) where T : new()
        {
            this.makeRequest(RestSharp.Method.POST, getResourceURL(path, null), parameters, callback);
        }


        private static String getResourceURL(String url)
        {
            // if it has a leading slash, remove it
            if (url.IndexOf("/") == 0) url = url.Substring(1);
            return url;
        }

        private static String getResourceURL(String path, Dictionary<String, String> parameters)
        {
            String url = getResourceURL(path);
            if (parameters != null)
            {
                // build a query string
                StringBuilder query = new StringBuilder();

                // Loop over pairs with foreach
                foreach (KeyValuePair<string, string> pair in parameters)
                {
                    query.Append(HttpUtility.UrlEncode(pair.Key));
                    query.Append("=");
                    query.Append(HttpUtility.UrlEncode(pair.Value));
                    query.Append("&");
                }
                url = String.Format("{0}?{1}", url, query);
            }
            return url;
        }

        /// <summary>
        /// Encapsulates the behaviour for asking the Authenticator for an access token. 
        /// This allows the request maker to disregard the authentication state when making requests.
        /// </summary>
        private class WordPressRestRequest
        {
            private RestRequest request = null;
            private WordPressRestClient wpRestClient = null;

            public WordPressRestRequest(WordPressRestClient wpRestClient, RestRequest request)
            {
                this.request = request;
                this.wpRestClient = wpRestClient;
            }

            /// <summary>
            /// Attempt to send the request, checks to see if we have an access token and if not
            /// asks the Authenticator to authenticate the request.
            /// 
            /// If no Authenticator is provided the request is always sent.
            /// </summary>
            public void send<T>(Action<IRestResponse<T>> callback) where T : new()
            {
                if (wpRestClient.IsAuthenticated || wpRestClient.WordPressAuthenticator == null)
                {
                    wpRestClient._client.ExecuteAsync(request, callback);
                }
                else
                {
                    wpRestClient.WordPressAuthenticator.authenticate(
                        (response) =>
                        {
                            Debug.WriteLine(response.Content);
                            if (response.ErrorException != null)
                            {
                                Debug.WriteLine("Auth Error");
                                RestResponse<T> outerResp = new RestResponse<T>();
                                outerResp.RawBytes = response.RawBytes;
                                outerResp.Content = response.Content;
                                outerResp.ContentEncoding = response.ContentEncoding;
                                outerResp.ContentLength = response.ContentLength;
                                outerResp.ContentType = response.ContentType;
                                
                                outerResp.ErrorException = response.ErrorException;
                                outerResp.ErrorMessage = response.ErrorMessage;
                                outerResp.Request = response.Request;
                                outerResp.ResponseUri = response.ResponseUri;
                                outerResp.Server = response.Server;
                                outerResp.StatusCode = response.StatusCode;
                                outerResp.StatusDescription = response.StatusDescription;
                                callback(outerResp);
                                return;
                            }

                            int respoCode = (int)response.StatusCode;

                            if (respoCode == 200)
                            {
                                Debug.WriteLine("Auth Completed");
                                wpRestClient.AccessToken = ((Token)response.Data).access_token;
                                wpRestClient._client.ExecuteAsync(request, callback);
                            }
                            else
                            {
                                Debug.WriteLine("Auth Error");
                                RestResponse<T> outerResp = new RestResponse<T>();
                                outerResp.RawBytes = response.RawBytes;
                                outerResp.Content = response.Content;
                                outerResp.ContentEncoding = response.ContentEncoding;
                                outerResp.ContentLength = response.ContentLength;
                                outerResp.ContentType = response.ContentType;
                                
                                outerResp.ErrorException = response.ErrorException;
                                outerResp.ErrorMessage = response.ErrorMessage;
                                outerResp.Request = response.Request;
                                outerResp.ResponseUri = response.ResponseUri;
                                outerResp.Server = response.Server;
                                outerResp.StatusCode = response.StatusCode;
                                outerResp.StatusDescription = response.StatusDescription;
                                callback(outerResp);
                                return;
                            }
                        }
                    );
                }
            }
        }
    }

    /// <summary>
    /// Interface that provides a method that should perform the necessary task to make sure
    /// the provided Request will be authenticated.
    /// 
    /// </summary>
    public interface IWordPressAuthenticator
    {
        void authenticate(Action<IRestResponse<Token>> callback);
    }

    public class WordPressOauthAuthenticator : IWordPressAuthenticator
    {
        public String UserName { get; set; }
        public String Password { get; set; }

        private String _oAuthAppID = null;
        private String _oAuthAppSecret = null;
        private String _oAuthRedirectURI = null;

        public WordPressOauthAuthenticator(String username, String password, String oAuthAppID, String oAuthAppSecret, String oAuthRedirectURI)
        {
            this.UserName = username;
            this.Password = password;
            this._oAuthAppID = oAuthAppID;
            this._oAuthAppSecret = oAuthAppSecret;
            this._oAuthRedirectURI = oAuthRedirectURI;
        }

        public void authenticate(Action<IRestResponse<Token>> callback)
        {

            RestClient _client = new RestClient();
            //defaults
            _client.Timeout = 30000;
            _client.FollowRedirects = true;
            _client.MaxRedirects = 3;
            _client.BaseUrl = OAuth.TOKEN_ENDPOINT;

            // silverlight friendly way to get current version
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            var version = assemblyName.Version;
            _client.UserAgent = "wordpress-csharp/" + version;
            OAuth oAuth = new OAuth(_oAuthAppID, _oAuthAppSecret, _oAuthRedirectURI);
            var oAuthRequest = oAuth.makePasswordRequest(UserName, Password);
            _client.ExecuteAsync<Token>(oAuthRequest, callback);
        }
    }

    public class Me
    {
        public string display_name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string avatar_URL { get; set; }
        public string profile_URL { get; set; }
    }

    public class Notes
    {
        public int number { get; set; }
        public int last_seen_time { get; set; }
        public List<Note> notes { get; set; }
    }

    public class Note
    {         
        public int id { get; set; }
        public int unread { get; set; }
        public string type { get; set; }
        public int timestamp { get; set; }
        public Dictionary<string, string> subject { get; set; }
        public Dictionary<string, string> body { get; set; }
    }

}