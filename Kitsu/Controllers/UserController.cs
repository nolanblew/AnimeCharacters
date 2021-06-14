using Kitsu.Models;
using Kitsu.Requests;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
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

        //public async Task<User> GetUserSelfAsync()
        //{
        //    if (!_restClient.HasAuthenticated())
        //    {
        //        throw new AuthenticationException("User has not authenticated. Use client.Login() to authenticate first.");
        //    }

        //    var request = _GetBaseRequest();
        //    request.AddQueryParameter(
        //        name: "filter[self]",
        //        value: "true",
        //        encode: false);

        //    var result = await _restClient.ExecuteGetAsync<UserGetResponse>(request);

        //    if (!result.IsSuccessful) { return null; }
        //    if (result.Data.data.Length != 1) { return null; }

        //    return new User(result.Data.data.First());
        //}
    }
}
