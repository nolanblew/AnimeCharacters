using Kitsu.Models;
using Kitsu.Requests;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Kitsu.Controllers
{
    public sealed class UserController : ControllerBase
    {
        internal UserController(string baseUrl) : base(baseUrl, "users") { }

        public async Task<User> GetUserAsync(string username)
        {
            var request = _GetBaseRequest();
            request.AddQueryParameter(
                name: "filter[slug]",
                value: username,
                encode: false);

            var resultContent = await ExecuteGetRequestAsync<UserGetResponse>(request);

            if (resultContent.data.Length != 1) { return null; }

            return new User(resultContent.data.First());
        }

        public async Task<User> GetUserSelfAsync(string authToken)
        {
            if (string.IsNullOrWhiteSpace(authToken))
            {
                throw new AuthenticationException("User has not authenticated. Use client.Login() to authenticate first.");
            }

            var request = _GetBaseRequest();
            request.AddQueryParameter(
                name: "filter[self]",
                value: "true",
                encode: false);

            request.AddHeader("Authorization", $"Bearer {authToken}");

            var result = await ExecuteGetRequestAsync<UserGetResponse>(request);

            if (result == null) { return null; }
            if (result.data.Length != 1) { return null; }

            return new User(result.data.First());
        }
    }
}
