using Kitsu.Models;
using RestSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsu.Controllers
{
    public class MediaController : ControllerBase
    {
        public MediaController(IRestClient restClient)
            : base(restClient, "anime") { }

        public async Task<Anime> GetAnimeById(string animeId)
        {
            var request = _GetBaseRequest("{id}");
            request.AddParameter("id", animeId, ParameterType.UrlSegment);

            request.AddQueryParameter(
                name: "include",
                value: "mappings",
                encode: false);

            request.AddQueryParameter(
                name: "fields[anime]",
                value: "canonicalTitle,posterImage,youtubeVideoId,slug,showType,mappings",
                encode: false);

            request.AddQueryParameter(
                name: "fields[manga]",
                value: "canonicalTitle",
                encode: false);

            var result = await _restClient.ExecuteGetTaskAsync<Responses.AnimeGetWithRelationshipsSlimResponse.Anime>(request);

            if (!result.IsSuccessful)
            {
                return null;
            }

            var myAnimeListId = result.Data.Mappings?.FirstOrDefault(m => m.Attributes.ExternalSite == "myanimelist/anime")?.Attributes.ExternalId;
            var convertedAnime = _ConvertToAnime(result.Data.Data, myAnimeListId);

            return convertedAnime;
        }

        public async Task<Anime> GetAnimeWithRelationships(int animeId)
        {
            var request = _GetBaseRequest("{id}");
            request.AddParameter("id", animeId.ToString(), ParameterType.UrlSegment);

            request.AddQueryParameter(
                name: "include",
                value: "mediaRelationships.destination",
                encode: false);

            request.AddQueryParameter(
                name: "fields[anime]",
                value: "canonicalTitle,youtubeVideoId,slug,posterImage,showType,mediaRelationships",
                encode: false);

            request.AddQueryParameter(
                name: "fields[manga]",
                value: "canonicalTitle",
                encode: false);

            var result = await _restClient.ExecuteGetTaskAsync<Responses.AnimeGetWithRelationshipsSlimResponse.Anime>(request);

            if (!result.IsSuccessful)
            {
                return null;
            }

            var relatedAnimes = result.Data.RelatedAnime.Select(_ConvertToAnime).ToList();
            var myAnimeListId = result.Data.Mappings?.FirstOrDefault(m => m.Attributes.ExternalSite == "myanimelist/anime")?.Attributes.ExternalId;

            var convertedAnime = _ConvertToAnime(result.Data.Data, myAnimeListId);
            convertedAnime.RelatedAnime.AddRange(relatedAnimes);

            return convertedAnime;
        }

        Anime _ConvertToAnime(Responses.AnimeGetWithRelationshipsSlimResponse.Data anime, string myAnimeListId = null)
        {
            if (anime == null) { return null; }

            return new Anime
            {
                KitsuId = anime.Id?.ToString(),
                MyAnimeListId = myAnimeListId,
                Title = anime.Attributes.CanonicalTitle,
                YoutubeId = anime.Attributes.YoutubeVideoId,
                KitsuSlug = anime.Attributes.Slug,
                PosterImageUrl = anime.Attributes.PosterImage.Large.AbsoluteUri,
                ShowType = Anime.ParseAnimeType(anime.Attributes.ShowType),
            };
        }

        Anime _ConvertToAnime(Responses.AnimeGetWithRelationshipsSlimResponse.Included anime)
        {
            if (anime == null) { return null; }

            return new Anime
            {
                KitsuId = anime.Id?.ToString(),
                Title = anime.Attributes.CanonicalTitle,
                YoutubeId = anime.Attributes.YoutubeVideoId,
                KitsuSlug = anime.Attributes.Slug,
                PosterImageUrl = anime.Attributes.PosterImage.Large.AbsoluteUri,
                ShowType = Anime.ParseAnimeType(anime.Attributes.ShowType),
            };
        }
    }
}
