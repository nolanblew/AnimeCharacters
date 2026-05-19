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
                CategoryName = "Anime",
                Description = "Find shared Japanese voice actors across anime in your Kitsu library.",
                IsCore = true,
                EnabledByDefault = true
            },
            new()
            {
                Id = BuiltInExtensionIds.GenshinImpact,
                Name = "Genshin Impact",
                CategoryName = "Video Games",
                Description = "Add playable Genshin Impact characters to shared voice actor results.",
                CoverImageUrl = "https://static.wikia.nocookie.net/gensin-impact/images/8/80/Genshin_Impact.png/revision/latest/scale-to-width-down/512",
                IsCore = false,
                EnabledByDefault = true
            }
        };

        public static IReadOnlyList<MediaExtensionDefinition> GetEnabledExtensions(UserSettings settings) =>
            All.Where(extension => settings?.IsExtensionEnabled(extension) ?? extension.EnabledByDefault).ToList();

        public static MediaExtensionDefinition GetById(string id) =>
            All.FirstOrDefault(extension => extension.Id == id);
    }
}
