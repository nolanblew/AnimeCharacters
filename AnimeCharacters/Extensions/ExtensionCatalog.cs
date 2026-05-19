using AnimeCharacters.Models;
using System.Collections.Generic;
using System.Linq;

namespace AnimeCharacters.Extensions
{
    /// <summary>
    /// Provides the built-in extension catalog and resolves user settings.
    /// </summary>
    public static class ExtensionCatalog
    {
        public static IReadOnlyList<MediaExtensionDefinition> All { get; } = new List<MediaExtensionDefinition>
        {
            new()
            {
                Id = BuiltInExtensionIds.KitsuLibrary,
                Name = "Kitsu Library",
                CategoryName = BuiltInExtensionCategories.Anime,
                Description = "Find shared Japanese voice actors across anime in your Kitsu library.",
                IsCore = true,
                EnabledByDefault = true
            },
            new()
            {
                Id = BuiltInExtensionIds.GenshinImpact,
                Name = "Genshin Impact",
                CategoryName = BuiltInExtensionCategories.VideoGames,
                Description = "Add playable Genshin Impact characters to shared voice actor results.",
                CoverImageUrl = "images/extensions/genshin-impact/cover.png",
                IsCore = false,
                EnabledByDefault = false
            }
        };

        public static IReadOnlyList<MediaExtensionDefinition> GetEnabledExtensions(UserSettings settings) =>
            All.Where(extension => settings?.IsExtensionEnabled(extension) ?? extension.EnabledByDefault).ToList();

        public static bool HasEnabledCategory(UserSettings settings, string categoryName) =>
            GetEnabledExtensions(settings).Any(extension => extension.CategoryName == categoryName);

        public static MediaExtensionDefinition GetById(string id) =>
            All.FirstOrDefault(extension => extension.Id == id);
    }
}
