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
            UserLibraryEvents = new LibraryEventsController(_baseApiUrl);
            //Anime = new MediaController(_baseApiUrl);
        }

        internal const string BASE_KITSU_URL = "https://kitsu.app";
        const string _baseApiUrl = BASE_KITSU_URL + "/api/edge/";


        public AuthorizationController Auth { get; }

        public UserController Users { get; }

        public LibraryController UserLibraries { get; }

        public LibraryEventsController UserLibraryEvents { get; }

        //public MediaController Anime { get; }

        //public bool HasAuthenticated => _client.Authenticator is OAuth2AuthorizationRequestHeaderAuthenticator;

        //public async Task Login(string username, string password)
        //{
        //    await _Auth.Login(_client, username, password);
        //}
    }
}
