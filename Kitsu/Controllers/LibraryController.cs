using Kitsu.Helpers;
using Kitsu.Models;
using Kitsu.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Kitsu.Controllers
{
    public class LibraryController : ControllerBase
    {
        public LibraryController(string baseUrl)
            : base(baseUrl, "library-entries") { }

        // https://kitsu.app/api/edge/library-entries?filter[userId]={{user_id}}&filter[kind]=anime&filter[status]=completed&include=anime,anime.mappings
        public async Task<List<LibraryEntry>> GetCompleteLibraryCollectionAsync(int userId, LibraryType type = LibraryType.All, LibraryStatus status = LibraryStatus.All)
        {
            var rtn = new List<LibraryEntry>();

            var request = _GetBaseRequest();

            request.AddQueryParameter(
                name: "filter[userId]",
                value: userId.ToString(),
                encode: false);

            var filterTypes = string.Join(",", type.MaskToList()).ToLower();
            var includeTypes = filterTypes;
            if (type.HasFlag(LibraryType.Anime))
            {
                includeTypes += ",anime.mappings";
            }

            request.AddQueryParameter(
                name: "filter[kind]",
                value: filterTypes,
                encode: false);

            request.AddQueryParameter(
                name: "include",
                value: includeTypes,
                encode: false);

            var statuses = string.Join(",", status.MaskToList()).ToLower();
            request.AddQueryParameter(
                name: "filter[status]",
                value: statuses,
                encode: false);

            while (request != null)
            {
                var restJson = await ExecuteGetRequestAsync(request);

                if (string.IsNullOrWhiteSpace(restJson))
                {
                    return null;
                }

                var result = UserLibraryGetRequest.FromJson(restJson);

                var listedResults = result.Data.Select(entry =>
                {
                    var includedAnime = result.Included
                        .FirstOrDefault(a => a.Id == entry.Relationships.Anime.Data.Id);

                    var mappingIds = includedAnime.Relationships.Mappings.Data.Select(map => map.Id).ToArray();

                    var myAnimeListId = result.Included
                        .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings
                            && mappingIds.Contains(inc.Id)
                            && inc.Attributes.ExternalSite == "myanimelist/anime")
                        ?.Attributes.ExternalId;

                    var anilistId = result.Included
                        .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings
                            && mappingIds.Contains(inc.Id)
                            && inc.Attributes.ExternalSite == "anilist/anime")
                        ?.Attributes.ExternalId
                        // If there is no 'anime' associated, it might be a data error or because there is a manga with the same info. If /anime isn't found, let's just search for anilist/
                        ?? result.Included
                        .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings
                            && mappingIds.Contains(inc.Id)
                            && inc.Attributes.ExternalSite.StartsWith("anilist/"))
                        ?.Attributes.ExternalId;

                    return includedAnime != null
                        ? new LibraryEntry(entry, type, includedAnime.Id.ToString(), myAnimeListId, anilistId, includedAnime.Attributes)
                        : new LibraryEntry(entry, type);
                }).ToList();

                rtn.AddRange(listedResults);
                request = null;

                if (!string.IsNullOrWhiteSpace(result.Links.Next?.ToString()))
                {
                    var url = HttpUtility.UrlDecode(result.Links.Next.ToString());
                    request = GetBaseRequest(url);
                }
            }

            return rtn;
        }

        public async Task<List<LibraryEntry>> GetLibraryCollectionByIdsAsync(int userId, long[] ids, LibraryType type = LibraryType.All, LibraryStatus status = LibraryStatus.All)
        {
            var rtn = new List<LibraryEntry>();

            var request = _GetBaseRequest();

            request.AddQueryParameter(
                name: "filter[userId]",
                value: userId.ToString(),
                encode: false);

            var filterTypes = string.Join(",", type.MaskToList()).ToLower();
            var includeTypes = filterTypes;
            if (type.HasFlag(LibraryType.Anime))
            {
                includeTypes += ",anime.mappings";
            }

            request.AddQueryParameter(
                name: "filter[kind]",
                value: filterTypes,
                encode: false);

            request.AddQueryParameter(
                name: "include",
                value: includeTypes,
                encode: false);

            var statuses = string.Join(",", status.MaskToList()).ToLower();
            request.AddQueryParameter(
                name: "filter[status]",
                value: statuses,
                encode: false);

            var idChunks = ids.Chunk(10);

            foreach (var idChunk in idChunks)
            {
                while (request != null)
                {
                    request.AddQueryParameter(
                        name: "filter[id]",
                        value: string.Join(",", idChunk),
                        encode: false);

                    var restJson = await ExecuteGetRequestAsync(request);

                    if (string.IsNullOrWhiteSpace(restJson))
                    {
                        return null;
                    }

                    var result = UserLibraryGetRequest.FromJson(restJson);

                    var listedResults = result.Data.Select(entry =>
                    {
                        var includedAnime = result.Included
                            .FirstOrDefault(a => a.Id == entry.Relationships.Anime.Data.Id);

                        var mappingIds = includedAnime.Relationships.Mappings.Data.Select(map => map.Id).ToArray();

                        var myAnimeListId = result.Included
                            .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings
                                && mappingIds.Contains(inc.Id)
                                && inc.Attributes.ExternalSite == "myanimelist/anime")
                            ?.Attributes.ExternalId;

                        var anilistId = result.Included
                            .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings
                                && mappingIds.Contains(inc.Id)
                                && inc.Attributes.ExternalSite == "anilist/anime")
                            ?.Attributes.ExternalId
                            ?? result.Included
                            // If there is no 'anime' associated, it might be a data error or because there is a manga with the same info. If /anime isn't found, let's just search for anilist/
                            .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings
                                && mappingIds.Contains(inc.Id)
                                && inc.Attributes.ExternalSite.StartsWith("anilist/"))
                            ?.Attributes.ExternalId;

                        return includedAnime != null
                            ? new LibraryEntry(entry, type, includedAnime.Id.ToString(), myAnimeListId, anilistId, includedAnime.Attributes)
                            : new LibraryEntry(entry, type);
                    }).ToList();

                    rtn.AddRange(listedResults);
                    request = null;

                    if (!string.IsNullOrWhiteSpace(result.Links.Next?.ToString()))
                    {
                        var url = HttpUtility.UrlDecode(result.Links.Next.ToString());
                        request = GetBaseRequest(url);
                    }
                }
            }

            return rtn;
        }
    }

    [Flags]
    public enum LibraryType
    {
        Anime = 1,
        Manga = 2 ^ 1,
        All = Anime | Manga
    }

    [Flags]
    public enum LibraryStatus
    {
        Current = 1,
        OnHold = 2 ^ 1,
        Completed = 2 ^ 2,
        All = Current | OnHold | Completed
    }
}
