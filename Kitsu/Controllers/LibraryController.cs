using Kitsu.Models;
using Kitsu.Responses;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Kitsu.Controllers
{
    public class LibraryController : ControllerBase
    {
        public LibraryController(IRestClient client)
            : base(client, "library-entries") { }

        // https://kitsu.io/api/edge/library-entries?filter[userId]={{user_id}}&filter[kind]=anime&filter[status]=completed&include=anime,anime.mappings
        public async Task<List<LibraryEntry>> GetLibraryCollectionAsync(int userId, LibraryType type = LibraryType.All, LibraryStatus status = LibraryStatus.All)
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
                var restResult = await _restClient.ExecuteGetTaskAsync(request);

                if (!restResult.IsSuccessful)
                {
                    return null;
                }

                var result = UserLibraryGetRequest.FromJson(restResult.Content);

                var listedResults = result.Data.Select(entry =>
                {
                    var includedAnime = result.Included
                        .FirstOrDefault(a => a.Id == entry.Relationships.Anime.Data.Id);
                    var myAnimeListId = result.Included
                        .FirstOrDefault(inc => inc.Type == UserLibraryGetRequest.DataType.Mappings && inc.Attributes.ExternalSite == "myanimelist/anime")
                        ?.Attributes.ExternalId;

                    if (includedAnime != null)
                    {
                        return new LibraryEntry(entry, type, includedAnime.Id.ToString(), myAnimeListId, includedAnime.Attributes);
                    }

                    return new LibraryEntry(entry, type);
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
