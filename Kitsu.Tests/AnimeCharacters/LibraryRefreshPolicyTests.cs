using AnimeCharacters.Helpers;
using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class LibraryRefreshPolicyTests
    {
        static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);

        [TestMethod]
        public void RequiresFullRefresh_ManualRefresh_UpdatesArtworkInOneClick()
        {
            Assert.IsTrue(RequiresRefresh(Now, manual: true));
        }

        [TestMethod]
        public void RequiresFullRefresh_MetadataOlderThanADay_RefreshesRegardlessOfProgressSync()
        {
            Assert.IsTrue(RequiresRefresh(Now.AddDays(-1)));
            Assert.IsFalse(RequiresRefresh(Now.AddHours(-23)));
        }

        [TestMethod]
        public void RequiresFullRefresh_LegacyCacheOrMigration_RefreshesMetadata()
        {
            Assert.IsTrue(RequiresRefresh(null));
            Assert.IsTrue(RequiresRefresh(Now, migrating: true));
            Assert.IsTrue(LibraryRefreshPolicy.RequiresFullRefresh(Array.Empty<LibraryEntry>(), false, false, Now));
        }

        static bool RequiresRefresh(DateTimeOffset? fetchedAt, bool manual = false, bool migrating = false) =>
            LibraryRefreshPolicy.RequiresFullRefresh(
                new[] { new LibraryEntry { Anime = new Anime { MetadataFetchedAt = fetchedAt } } },
                manual, migrating, Now);
    }
}
