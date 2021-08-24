using Newtonsoft.Json;

namespace AniListClient.Queries
{
    internal static class StaffQueries
    {

        internal const string GET_STAFF_BY_ID_NAME = "getStaffByAnime";

        internal const string GET_STAFF_BY_ID_QUERY = @"
query getStaffByAnime($staff_id: Int, $page:Int=1) {
	Staff(id: $staff_id) {
    id
    name {
      first
      middle
      last
      full
      native
      userPreferred
    }
    languageV2
    image {
      large
      medium
    }
    description
    characters(page: $page) {
      edges {
        id
        role
        node {
          name {
            first
            middle
            last
            full
            native
            userPreferred
          }
          image {
            large
            medium
          }
          description
        }
        media {
          id
          title {
            romaji
            english
            native
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
    }

    internal class StaffQueryVariable : IHasPage
    {
        internal StaffQueryVariable() { }

        internal StaffQueryVariable(int staffId)
        {
            StaffId = staffId;
        }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("staff_id")]
        public int StaffId { get; set; }
    }
}
