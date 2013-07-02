# WordPress REST Client for C#

## Usage

Easiest way to use the library is by checking out the project and referencing it into your solution.
You should also install the Rest Sharp library into your project by getting it with NuGet Packages manager (Refence id: RestSharp).

	WordPressOauthAuthenticator oAuthAuthenticator = new WordPressOauthAuthenticator("username", "pass", "oAuthAppId", "oAuthAppSecret", "oAuthAppRedirect");
	WordPressRestClient wpRestClient = new WordPressRestClient(oAuthAuthenticator);
	wpRestClient.get<WordPress_csharp_REST_client.Me>("me", `callback`);
	wpRestClient.getNotifications<WordPress_csharp_REST_client.Notes>(`callback`);