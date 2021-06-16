using Kitsu.Models;
using Kitsu.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Kitsu.Controllers
{
    public class LibraryEventsController : ControllerBase
    {
        public LibraryEventsController(string baseUrl)
            : base(baseUrl, "library-events") { }

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
                value: "-id",
                encode: false);
            request.AddQueryParameter(
                name: "fields[library-events]",
                value: "createdAt,changedData,kind,libraryEntry",
                encode: false);
            request.AddQueryParameter(
                name: "include",
                value: "libraryEntry",
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

                foreach (var entry in result.Data.Where(d => d.Attributes.Kind != null)) // For unexpected "kinds". Should probably make note of these
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
                    if (!usedLibraryIds.Contains(includedLibraryData.Id))
                    {
                        rtn.Add(new(entry, includedLibraryData));
                        usedLibraryIds.Add(includedLibraryData.Id);
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

        public async Task<long?> GetLatestLibraryEventId(int userId)
        {
            var request = _GetBaseRequest();

            request.AddQueryParameter(
                name: "filter[userId]",
                value: userId.ToString(),
                encode: false);
            request.AddQueryParameter(
                name: "sort",
                value: "-id",
                encode: false);
            request.AddQueryParameter(
                name: "fields[library-events]",
                value: "id",
                encode: false);
            request.AddQueryParameter(
                name: "page[limit]",
                value: "1",
                encode: false);

            var result = await ExecuteGetRequestAsync<UserLibraryEventIdOnlyGetResponse>(request);

            return result?.Data?.FirstOrDefault()?.Id;
        }
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
