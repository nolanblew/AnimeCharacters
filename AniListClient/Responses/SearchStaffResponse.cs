using Newtonsoft.Json;
using static AniListClient.Responses.AniListClient.Responses.StaffQueryResponse;

namespace AniListClient.Responses
{
    internal class SearchStaffResponse
    {
        [JsonProperty("Page", NullValueHandling = NullValueHandling.Ignore)]
        public PageResponse Page { get; set; }

        internal class PageResponse
        {
            [JsonProperty("staff", NullValueHandling = NullValueHandling.Ignore)]
            public StaffResponse[] Staff { get; set; }
        }
    }
}
