using AniListClient.Converters;
using AniListClient.Models;
using AniListClient.Queries;
using AniListClient.Responses;
using GraphQL.Client.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public class CharactersController : BaseController
    {
        public CharactersController(GraphQLHttpClient graphQLHttpClient)
            : base(graphQLHttpClient) { }

        public Task<List<Character>> GetCharactersById(int id) =>
            GetPaginatedList<CharactersFromAnimeResponse, Character>(
                CharactersQueries.GET_STAFF_BY_ANIME_ID_QUERY,
                CharactersQueries.GET_STAFF_BY_ANIME_ID_NAME,
                new CharacterQueryVariable(id),
                q => q.Media.Characters.Edges.Select(c => c.ToCharacter()).ToList(),
                q => q.Media.Characters.PageInfo?.HasNextPage ?? false);
    }
}
