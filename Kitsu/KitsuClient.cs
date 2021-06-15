using Kitsu.Controllers;

namespace Kitsu
{
    public class KitsuClient
    {
        public KitsuClient()
        {

            Auth = new AuthorizationController();
            Users = new UserController(_baseApiUrl);
            UserLibraries = new LibraryController(_baseApiUrl);
            //Anime = new MediaController(_baseApiUrl);
        }

        const string _baseApiUrl = "https://kitsu.io/api/edge/";


        public AuthorizationController Auth { get; }

        public UserController Users { get; }

        public LibraryController UserLibraries { get; }

        //public MediaController Anime { get; }

        //public bool HasAuthenticated => _client.Authenticator is OAuth2AuthorizationRequestHeaderAuthenticator;

        //public async Task Login(string username, string password)
        //{
        //    await _Auth.Login(_client, username, password);
        //}
    }
}
