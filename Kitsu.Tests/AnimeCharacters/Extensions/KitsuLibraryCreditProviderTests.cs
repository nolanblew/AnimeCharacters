using AnimeCharacters.Extensions;
using AnimeCharacters.Models;
using AniListClient.Models;
using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReferenceApis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kitsu.Tests.AnimeCharacters.Extensions
{
    [TestClass]
    public class KitsuLibraryCreditProviderTests
    {
        [TestMethod]
        public async Task GetCreditsAsync_WhenReferenceMediaOmitsTitles_UsesLibraryTitle()
        {
            var libraryEntry = new LibraryEntry
            {
                Anime = new Anime
                {
                    KitsuId = "library-anime",
                    AnilistId = "777",
                    Title = "Library Canonical",
                    EnglishTitle = "Library English"
                }
            };
            var database = new StubDatabaseProvider(new[] { libraryEntry });
            var provider = new KitsuLibraryCreditProvider(database, new StubReferenceAnimeService());
            var staff = new Staff(
                1,
                new Names(null, null, null, "Voice Actor", null, null, null),
                Language.Japanese,
                null,
                null,
                null,
                null,
                null,
                null,
                new List<Character>
                {
                    new(
                        2,
                        new Names(null, null, null, "Character", null, null, null),
                        null,
                        null,
                        CharacterRole.Main,
                        new List<MediaBase> { new(777, null) },
                        null)
                });

            var credits = await provider.GetCreditsAsync(staff);

            Assert.AreEqual(1, credits.Count);
            Assert.AreEqual("Library English", credits[0].MediaTitle);
        }

        class StubReferenceAnimeService : IReferenceAnimeService
        {
            public IEnumerable<ReferenceAnimeKey> GetKnownAnimeKeys(Anime anime) =>
                new[] { new ReferenceAnimeKey(ReferenceProviderNames.AniList, anime.AnilistId) };

            public Task<ReferenceMediaResult> GetMediaWithCharactersAsync(Anime anime, IReadOnlyCollection<string> searchTitles) =>
                throw new NotImplementedException();

            public Task<Staff> GetStaffByIdAsync(string providerName, string id, string fallbackName = null) =>
                throw new NotImplementedException();
        }

        class StubDatabaseProvider : global::AnimeCharacters.IDatabaseProvider
        {
            readonly IList<LibraryEntry> _libraries;

            public StubDatabaseProvider(IList<LibraryEntry> libraries)
            {
                _libraries = libraries;
            }

            public ValueTask<IList<LibraryEntry>> GetLibrariesAsync() => ValueTask.FromResult(_libraries);
            public ValueTask<UserSettings> GetUserSettingsAsync() => ValueTask.FromResult(new UserSettings());
            public ValueTask<Kitsu.Models.User> GetUserAsync() => throw new NotImplementedException();
            public ValueTask SetUserAsync(Kitsu.Models.User value) => throw new NotImplementedException();
            public ValueTask<long?> GetLastFetchedIdAsnyc() => throw new NotImplementedException();
            public ValueTask SetLastFetchedIdAsync(long? value) => throw new NotImplementedException();
            public ValueTask<DateTimeOffset?> GetLastFetchedDateAsnyc() => throw new NotImplementedException();
            public ValueTask SetLastFetchedDateAsync(DateTimeOffset value) => throw new NotImplementedException();
            public ValueTask SetLibrariesAsync(IList<LibraryEntry> value) => throw new NotImplementedException();
            public ValueTask<int?> GetMigrationVersionAsnyc() => throw new NotImplementedException();
            public ValueTask SetMigrationVersionAsync(int value) => throw new NotImplementedException();
            public ValueTask SetUserSettingsAsync(UserSettings value) => throw new NotImplementedException();
            public ValueTask ClearAsync() => throw new NotImplementedException();
        }
    }
}
