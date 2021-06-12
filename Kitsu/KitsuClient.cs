using Kitsu.Controllers;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization;
using System.Threading.Tasks;

namespace Kitsu
{
    public class KitsuClient
    {
        public KitsuClient()
        {

            //_Auth = new AuthorizationController();
            Users = new UserController(_baseApiUrl);
            //UserLibraries = new LibraryController(_baseApiUrl);
            //Anime = new MediaController(_baseApiUrl);
        }

        const string _clientId = "dd031b32d2f56c990b1425efe6c42ad847e7fe3ab46bf1299f05ecd856bdb7dd";
        const string _clientSecret = "54d7307928f63414defd96399fc31ba847961ceaecef3a5fd93144e960c0e151";

        const string _baseApiUrl = "https://kitsu.io/api/edge/";


        //AuthorizationController _Auth { get; }

        public UserController Users { get; }

        //public LibraryController UserLibraries { get; }

        //public MediaController Anime { get; }

        //public bool HasAuthenticated => _client.Authenticator is OAuth2AuthorizationRequestHeaderAuthenticator;

        //public async Task Login(string username, string password)
        //{
        //    await _Auth.Login(_client, username, password);
        //}
    }

    class JsonDeserializer : IRestSerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string ContentType { get; set; } = "application/json";

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public string Serialize(Parameter parameter)
        {
            return this.Serialize(parameter.Value);
        }

        public string[] SupportedContentTypes => RestSharp.Serialization.ContentType.JsonAccept;

        public DataFormat DataFormat => DataFormat.Json;
    }
}
