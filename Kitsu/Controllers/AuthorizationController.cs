using Kitsu.Responses;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Kitsu.Controllers
{
    public sealed class AuthorizationController
    {
        public AuthorizationController()
        {
            _oauthClient = new RestClient("https://kitsu.io/api/oauth/");
        }

        readonly IRestClient _oauthClient;

        public async Task<AuthorizationResponse> Login(IRestClient restClient, string username, string password)
        {
            var result = await Login(username, password);
            if (result != null)
            {
                restClient.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(result.access_token, result.token_type);
            }

            return result;
        }

        public async Task<AuthorizationResponse> Login(string username, string password)
        {
            var request = new RestRequest("token");
            request.AddHeader("Accept", "application/vnd.api+json");

            request.AddQueryParameter("grant_type", "password");
            request.AddQueryParameter("username", username);
            request.AddQueryParameter("password", password);

            var url = _oauthClient.BuildUri(request);

            var result = await _oauthClient.ExecutePostTaskAsync<AuthorizationResponse>(request);

            if (!result.IsSuccessful)
            {
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new InvalidCredentialException("Username and password do not match.");
                }
                else
                {
                    throw result.ErrorException;
                }
            }

            return result.Data;
        }
    }
}
