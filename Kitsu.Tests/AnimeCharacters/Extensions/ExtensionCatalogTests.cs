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
        public void GetEnabledExtensions_WhenSettingsAreEmpty_KeepsCoreKitsuAndGenshinEnabled()
        {
            var settings = new UserSettings();

            var enabledExtensions = ExtensionCatalog.GetEnabledExtensions(settings).ToList();

            CollectionAssert.Contains(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.KitsuLibrary);
            CollectionAssert.Contains(enabledExtensions.Select(extension => extension.Id).ToList(), BuiltInExtensionIds.GenshinImpact);
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
    }
}
