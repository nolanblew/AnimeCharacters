using AniListClient.Converters;
using AniListClient.Models;
using AniListClient.Queries;
using AniListClient.Responses;
using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public class CharactersController : BaseController
    {
        public CharactersController(GraphQLHttpClient graphQLHttpClient)
            : base(graphQLHttpClient) { }

        public Task<Media> GetMediaWithCharactersById(int id) =>
            GetPaginatedList<CharactersFromAnimeResponse, Media, Character>(
                CharactersQueries.GET_STAFF_BY_ANIME_ID_QUERY,
                CharactersQueries.GET_STAFF_BY_ANIME_ID_NAME,
                new CharacterQueryVariable(id),
                q => q.Characters,
                q => q.Media.ToMedia(),
                q => q.Media.Characters.PageInfo?.HasNextPage ?? false);

        public async Task<MediaBase> SearchMediaByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var request = new GraphQLRequest(CharactersQueries.SEARCH_ANIME_BY_TITLE_QUERY)
            {
                OperationName = CharactersQueries.SEARCH_ANIME_BY_TITLE_NAME,
                Variables = new SearchAnimeQueryVariable(title)
            };

            GraphQLResponse<SearchMediaResponse> result;
            try
            {
                result = await _graphQLHttpClient.SendQueryAsync<SearchMediaResponse>(request);
            }
            catch (GraphQLHttpRequestException ex)
            {
                throw new InvalidOperationException(GetGraphQLHttpErrorMessage(ex), ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Unable to reach AniList. The API may be unavailable right now.", ex);
            }

            if (result.Errors?.Length > 0)
            {
                throw new InvalidOperationException(result.Errors[0].Message);
            }

            return result.Data?.Page?.Media?.FirstOrDefault()?.ToMedia();
        }
    }
}
