using AniListClient.Models;
using GraphQL.Client.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AniListClient.Controllers
{
    public class CharactersController : BaseController
    {
        public CharactersController(GraphQLHttpClient graphQLHttpClient)
            : base(graphQLHttpClient) { }

        #region QUERIES

        const string _getStafByAnimeQuery = @"
query getStaffByAnime($anime_id: Int, $page:Int=1) {
  Media(id:$anime_id) {
    id
    title {
      romaji
      english
      native
      userPreferred
    }
    description
    coverImage {
      extraLarge
      large
      color
    }
    status
    characters(page: $page) {
      edges { # Array of character edges
        node { # Character node
          id
          name {
            first
            last
          }
          image {
            large
          }
        }
        role
        voiceActors(language:JAPANESE) { # Array of voice actors of this character for the anime
          id
          name {
            first
            last
            userPreferred
          }
        }
      }
      pageInfo {
        total
        perPage
        currentPage
        lastPage
        hasNextPage
      }
    }
  }
}";

        #endregion

        public List<Character> GetCharactersById(int id)
        {

        }
    }
}
