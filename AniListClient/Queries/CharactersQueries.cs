using AniListClient.Models;
using Newtonsoft.Json;

namespace AniListClient.Queries
{
    internal static class CharactersQueries
    {
        internal const string GET_STAFF_BY_ANIME_ID_NAME = "getStaffByAnime";

        internal const string GET_STAFF_BY_ANIME_ID_QUERY = @"
query getStaffByAnime($anime_id:Int, $language:StaffLanguage=JAPANESE, $page:Int=1) {
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
      medium
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
            full
          }
          image {
            large
            medium
          }
        }
        role
        voiceActors(language:$language) { # Array of voice actors of this character for the anime
          id
          name {
            first
            last
            full
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
    }

    internal class CharacterQueryVariable : IHasPage
    {
        internal CharacterQueryVariable() { }

        internal CharacterQueryVariable(int animeId)
        {
            AnimeId = animeId;
        }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("anime_id")]
        public int AnimeId { get; set; }

        [JsonProperty("language")]
        public Language Language { get; set; } = Language.Japanese;
    }
}
