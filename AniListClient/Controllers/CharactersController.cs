using AniListClient.Converters;
using AniListClient.Models;
using AniListClient.Queries;
using AniListClient.Responses;
using GraphQL.Client.Http;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public class CharactersController : BaseController
    {
        public CharactersController(GraphQLHttpClient graphQLHttpClient)
            : base(graphQLHttpClient) { }

        public Task<Media> GetMediaWithCharactersById(int id, Language language) =>
            GetPaginatedList<CharactersFromAnimeResponse, Media, Character>(
                CharactersQueries.GET_STAFF_BY_ANIME_ID_QUERY,
                CharactersQueries.GET_STAFF_BY_ANIME_ID_NAME,
                new CharacterQueryVariable(id, language),
                q => q.Characters,
                q => q.Media.ToMedia(),
                q => q.Media.Characters.PageInfo?.HasNextPage ?? false);
    }
}
