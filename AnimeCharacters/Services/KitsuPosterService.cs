using AnimeCharacters.Helpers;
using Kitsu.Responses;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>Renews Kitsu poster links and coalesces repeated requests for the same anime.</summary>
    public class KitsuPosterService
    {
        readonly HttpClient _httpClient;
        readonly ILogger<KitsuPosterService> _logger;
        readonly Dictionary<string, (DateTimeOffset StartedAt, Task<IReadOnlyList<string>> Request)> _requests = new();

        public KitsuPosterService(HttpClient httpClient, ILogger<KitsuPosterService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>Fetches current poster sizes using the exact Kitsu anime ID.</summary>
        public Task<IReadOnlyList<string>> GetPosterUrlsAsync(string kitsuId)
        {
            if (!long.TryParse(kitsuId, out var id) || id <= 0)
            {
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            }

            var now = DateTimeOffset.UtcNow;
            if (_requests.TryGetValue(kitsuId, out var existing)
                && now - existing.StartedAt < TimeSpan.FromMinutes(1))
            {
                return existing.Request;
            }

            var request = FetchPosterUrlsAsync(id);
            _requests[kitsuId] = (now, request);
            return request;
        }

        /// <summary>Clears cached renewal results so an explicit refresh can recover from failures.</summary>
        public void ClearCache() => _requests.Clear();

        async Task<IReadOnlyList<string>> FetchPosterUrlsAsync(long id)
        {
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://kitsu.app/api/edge/anime/{id}?fields%5Banime%5D=posterImage");
                request.Headers.Accept.ParseAdd("application/vnd.api+json");
                request.SetBrowserRequestCache(BrowserRequestCache.NoStore);
                using var response = await _httpClient.SendAsync(request, timeout.Token);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync(timeout.Token);
                var poster = JObject.Parse(json)["data"]?["attributes"]?["posterImage"]
                    ?.ToObject<UserLibraryGetRequest.PosterImage>();

                return new[] { poster?.Large, poster?.Medium, poster?.Original, poster?.Small, poster?.Tiny }
                    .Where(url => url != null)
                    .Select(url => url.AbsoluteUri)
                    .Where(url => !ImageUrlHelper.NeedsRefresh(url, DateTimeOffset.UtcNow))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException
                || ex is Newtonsoft.Json.JsonException)
            {
                _logger.LogWarning("Could not renew the poster for Kitsu anime {AnimeId}: {ErrorType}",
                    id, ex.GetType().Name);
                return Array.Empty<string>();
            }
        }
    }
}
