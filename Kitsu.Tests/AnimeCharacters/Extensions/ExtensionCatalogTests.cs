using AnimeCharacters.Extensions;
using AnimeCharacters.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Kitsu.Tests.AnimeCharacters.Extensions
{
    [TestClass]
    public class ExtensionCatalogTests
    {
        [TestMethod]
        public void GetEnabledExtensions_WhenSettingsAreEmpty_KeepsCoreKitsuAndGenshinDisabled()
        {
            var settings = new UserSettings();

            var enabledExtensions = ExtensionCatalog.GetEnabledExtensions(settings).ToList();

            CollectionAssert.Contains(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.KitsuLibrary);
            CollectionAssert.DoesNotContain(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.GenshinImpact);
        }

        [TestMethod]
        public void GetEnabledExtensions_WhenGenshinIsEnabled_IncludesGenshin()
        {
            var settings = new UserSettings();
            settings.SetExtensionEnabled(BuiltInExtensionIds.GenshinImpact, true);

            var enabledExtensions = ExtensionCatalog.GetEnabledExtensions(settings).ToList();

            CollectionAssert.Contains(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.GenshinImpact);
        }

        [TestMethod]
        public void HasEnabledCategory_WhenGenshinIsNotEnabled_ReturnsFalseForVideoGames()
        {
            var settings = new UserSettings();

            var hasVideoGames = ExtensionCatalog.HasEnabledCategory(settings, BuiltInExtensionCategories.VideoGames);

            Assert.IsFalse(hasVideoGames);
        }

        [TestMethod]
        public void HasEnabledCategory_WhenGenshinIsEnabled_ReturnsTrueForVideoGames()
        {
            var settings = new UserSettings();
            settings.SetExtensionEnabled(BuiltInExtensionIds.GenshinImpact, true);

            var hasVideoGames = ExtensionCatalog.HasEnabledCategory(settings, BuiltInExtensionCategories.VideoGames);

            Assert.IsTrue(hasVideoGames);
        }

        [TestMethod]
        public void GetEnabledExtensions_WhenGenshinIsDisabled_RemovesGenshinButKeepsCoreKitsu()
        {
            var settings = new UserSettings();
            settings.SetExtensionEnabled(BuiltInExtensionIds.KitsuLibrary, false);
            settings.SetExtensionEnabled(BuiltInExtensionIds.GenshinImpact, false);

            var enabledExtensions = ExtensionCatalog.GetEnabledExtensions(settings).ToList();

            CollectionAssert.Contains(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.KitsuLibrary);
            CollectionAssert.DoesNotContain(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.GenshinImpact);
        }

        [TestMethod]
        public void GetById_ForGenshinImpact_IncludesCoverArt()
        {
            var extension = ExtensionCatalog.GetById(BuiltInExtensionIds.GenshinImpact);

            Assert.IsFalse(string.IsNullOrWhiteSpace(extension.CoverImageUrl));
            Assert.AreEqual("images/extensions/genshin-impact/cover.png", extension.CoverImageUrl);
        }
    }
}
