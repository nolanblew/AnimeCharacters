using Newtonsoft.Json;

namespace AniListClient.Responses
{
    internal class SearchMediaResponse
    {
        [JsonProperty("Page", NullValueHandling = NullValueHandling.Ignore)]
        public PageResponse Page { get; set; }

        internal class PageResponse
        {
            [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
            public MediaResponse[] Media { get; set; }
        }

        internal class MediaResponse
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public Title Title { get; set; }
        }
    }
}
