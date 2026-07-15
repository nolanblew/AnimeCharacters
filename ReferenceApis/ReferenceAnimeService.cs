using AniListClient.Models;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReferenceApis
{
    public class ReferenceAnimeService : IReferenceAnimeService
    {
        readonly IReadOnlyList<IReferenceAnimeProvider> _providers;
        readonly Dictionary<string, ReferenceMediaResult> _mediaCache = new();
        readonly Dictionary<string, Staff> _staffCache = new();

        public ReferenceAnimeService(IEnumerable<IReferenceAnimeProvider> providers)
        {
            _providers = providers?.ToList() ?? new List<IReferenceAnimeProvider>();
        }

        public IEnumerable<ReferenceAnimeKey> GetKnownAnimeKeys(Anime anime) =>
            _providers
                .Select(provider => provider.GetKnownAnimeKey(anime))
                .Where(key => key != null)
                .GroupBy(key => key.CacheKey)
                .Select(group => group.First());

        public async Task<ReferenceMediaResult> GetMediaWithCharactersAsync(
            Anime anime,
            IReadOnlyCollection<string> searchTitles)
        {
            if (anime == null)
            {
                throw new ArgumentNullException(nameof(anime));
            }

            var failures = new List<string>();

            foreach (var provider in _providers)
            {
                var knownKey = provider.GetKnownAnimeKey(anime);
                if (knownKey != null && _mediaCache.TryGetValue(knownKey.CacheKey, out var cachedMedia))
                {
                    return cachedMedia;
                }

                try
                {
                    var result = await provider.GetMediaWithCharactersAsync(anime, searchTitles);

                    if (result?.Media == null)
                    {
                        failures.Add($"{provider.DisplayName} did not return media data.");
                        continue;
                    }

                    _mediaCache[result.AnimeKey.CacheKey] = result;
                    return result;
                }
                catch (Exception ex) when (ex is ReferenceApiProviderException || ex is HttpRequestException || ex is InvalidOperationException || ex is TaskCanceledException)
                {
                    failures.Add($"{provider.DisplayName}: {ex.Message}");
                }
            }

            var details = failures.Any()
                ? $" Tried {string.Join(" ", failures)}"
                : " No reference API providers are configured.";

            throw new InvalidOperationException($"Unable to load character reference data.{details}");
        }

        public async Task<Staff> GetStaffByIdAsync(string providerName, string id, string fallbackName = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A staff id is required.", nameof(id));
            }

            var provider = _providers.FirstOrDefault(provider =>
                ReferenceAnimeKey.MatchesProvider(provider.Name, providerName));

            if (provider == null)
            {
                throw new InvalidOperationException($"Unknown reference API provider '{providerName}'.");
            }

            var cacheKey = $"{ReferenceAnimeKey.NormalizeProviderName(provider.Name)}:{id}";
            if (_staffCache.TryGetValue(cacheKey, out var cachedStaff))
            {
                return cachedStaff;
            }

            try
            {
                var staff = await provider.GetStaffByIdAsync(id);
                _staffCache[cacheKey] = staff;
                return staff;
            }
            catch (Exception primaryException) when (IsProviderFailure(primaryException) && !string.IsNullOrWhiteSpace(fallbackName))
            {
                var failures = new List<string> { $"{provider.DisplayName}: {primaryException.Message}" };

                foreach (var fallbackProvider in _providers.Where(candidate => candidate != provider))
                {
                    try
                    {
                        var staff = await fallbackProvider.FindStaffByNameAsync(fallbackName);
                        if (staff == null)
                        {
                            failures.Add($"{fallbackProvider.DisplayName} did not find an exact staff match.");
                            continue;
                        }

                        _staffCache[cacheKey] = staff;
                        _staffCache[$"{ReferenceAnimeKey.NormalizeProviderName(fallbackProvider.Name)}:{staff.Id}"] = staff;
                        return staff;
                    }
                    catch (Exception fallbackException) when (IsProviderFailure(fallbackException))
                    {
                        failures.Add($"{fallbackProvider.DisplayName}: {fallbackException.Message}");
                    }
                }

                throw new InvalidOperationException(
                    $"Unable to load staff reference data. Tried {string.Join(" ", failures)}",
                    primaryException);
            }
        }

        static bool IsProviderFailure(Exception exception) =>
            exception is ReferenceApiProviderException
            || exception is HttpRequestException
            || exception is InvalidOperationException
            || exception is TaskCanceledException;
    }
}
