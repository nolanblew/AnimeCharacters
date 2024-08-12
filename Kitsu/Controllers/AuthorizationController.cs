using Kitsu.Responses;
using System;
using System.Threading.Tasks;

namespace Kitsu.Controllers
{
    public sealed class AuthorizationController : ControllerBase
    {
        const string _clientId = "dd031b32d2f56c990b1425efe6c42ad847e7fe3ab46bf1299f05ecd856bdb7dd";
        const string _clientSecret = "54d7307928f63414defd96399fc31ba847961ceaecef3a5fd93144e960c0e151";

        public AuthorizationController()
            : base(KitsuClient.BASE_KITSU_URL + "/api/oauth/", "") { }

        public async Task<AuthorizationResponse> Login(string username, string password)
        {
            var request = _GetRelativeRequest("token");
            request.AddHeader("Accept", "application/vnd.api+json");

            request.AddQueryParameter("grant_type", "password");
            request.AddQueryParameter("username", username);
            request.AddQueryParameter("password", password);

            var result = await ExecutePostRequestAsync<AuthorizationResponse>(request);

            if (result == null)
            {
                throw new UnauthorizedAccessException("Username and password do not match.");
            }

            return result;
        }
    }
}
