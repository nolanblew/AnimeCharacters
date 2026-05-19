using System;

namespace ReferenceApis
{
    public sealed record ReferenceAnimeKey(string ProviderName, string Id)
    {
        public string CacheKey => $"{NormalizeProviderName(ProviderName)}:{Id}";

        public static string NormalizeProviderName(string providerName) =>
            providerName?.Trim().ToLowerInvariant() ?? string.Empty;

        public static bool MatchesProvider(string providerName, string expectedProviderName) =>
            string.Equals(
                NormalizeProviderName(providerName),
                NormalizeProviderName(expectedProviderName),
                StringComparison.OrdinalIgnoreCase);
    }
}
