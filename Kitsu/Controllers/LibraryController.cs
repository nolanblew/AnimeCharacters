﻿using Kitsu.Models;
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

        // https://kitsu.io/api/edge/library-entries?filter[userId]={{user_id}}&filter[kind]=anime&filter[status]=completed&include=anime,anime.mappings
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

                    return includedAnime != null
                        ? new LibraryEntry(entry, type, includedAnime.Id.ToString(), myAnimeListId, includedAnime.Attributes)
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

        public async Task<(long LastFetchedId, List<LibraryEntryEvent> LibraryEntryEvents)> GetLibraryEventsSinceTime(int userId, long lastFetchedId, int maxEntries = 500)
        {
            var rtn = new List<LibraryEntryEvent>();

            var request = _GetBaseRequest();

            var pageLimit = 20;
            var totalEntries = 0;
            long mostRecentId = -1;

            HashSet<long> usedLibraryIds = new();

            request.AddQueryParameter(
                name: "filter[userId]",
                value: userId.ToString(),
                encode: false);
            request.AddQueryParameter(
                name: "sort",
                value: "-createdAt",
                encode: false);
            request.AddQueryParameter(
                name: "fields[library-events]",
                value: "createdAt,changedData,kind,libraryEntry",
                encode: false);
            request.AddQueryParameter(
                name: "include",
                value: "include",
                encode: false);
            request.AddQueryParameter(
                name: "page[limit]",
                value: pageLimit.ToString(),
                encode: false);

            while (request != null)
            {
                var restJson = await ExecuteGetRequestAsync(request);

                if (string.IsNullOrWhiteSpace(restJson))
                {
                    return (mostRecentId, rtn);
                }

                var result = UserLibraryEventGetResponse.FromJson(restJson);

                foreach (var entry in result.Data)
                {
                    if (totalEntries > maxEntries)
                    {
                        throw new RequestCapacityExceededException(maxEntries);
                    }

                    totalEntries++;

                    if (entry.Id <= lastFetchedId)
                    {
                        return (mostRecentId, rtn);
                    }

                    if (entry.Id > mostRecentId)
                    {
                        mostRecentId = entry.Id.Value;
                    }

                    var includedLibraryData = result.Included
                        .FirstOrDefault(a => a.Id == entry.Relationships.LibraryEntry.Data.Id);

                    // If we already have seen this library, we have the latest version of it so
                    // don't try to add another event.
                    if (!usedLibraryIds.Contains(includedLibraryData.Id.Value))
                    {
                        rtn.Add(new(entry, includedLibraryData));
                        usedLibraryIds.Add(includedLibraryData.Id.Value);
                    }
                }

                request = null;

                if (!string.IsNullOrWhiteSpace(result.Links.Next?.ToString()))
                {
                    var url = HttpUtility.UrlDecode(result.Links.Next.ToString());
                    request = GetBaseRequest(url);
                }
            }

            return (mostRecentId, rtn); // Should only hit if we are a new account
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

    public class RequestCapacityExceededException : Exception
    {
        public RequestCapacityExceededException(int requests)
        {
            Requests = requests;
        }

        public int Requests { get; }
    }
}
