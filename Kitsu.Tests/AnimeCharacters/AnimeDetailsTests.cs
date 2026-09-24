using AnimeCharacters;
using AnimeCharacters.Models;
using AnimeCharacters.Pages;
using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReferenceApis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class AnimeDetailsTests
    {
        [TestMethod]
        public async Task PersistResolvedReferenceId_WhenTenraiReturnsMalId_SavesMissingMyAnimeListId()
        {
            var anime = new Anime
            {
                KitsuId = "49265",
                MyAnimeListId = null,
                AnilistId = "182616"
            };
            var libraries = new List<LibraryEntry> { new() { Anime = anime } };
            var database = new StubDatabaseProvider();
            var page = new AnimeDetails { CurrentAnime = anime };
            SetDatabaseProvider(page, database);

            await InvokePersistResolvedReferenceId(
                page,
                new ReferenceMediaResult(
                    new ReferenceAnimeKey(ReferenceProviderNames.Tenrai, "60059"),
                    null),
                libraries);

            Assert.AreEqual("60059", anime.MyAnimeListId);
            Assert.AreEqual("182616", anime.AnilistId);
            Assert.AreSame(libraries, database.SavedLibraries);
        }

        static Task InvokePersistResolvedReferenceId(
            AnimeDetails page,
            ReferenceMediaResult result,
            IList<LibraryEntry> libraries)
        {
            var method = typeof(AnimeDetails).GetMethod(
                "_PersistResolvedReferenceId",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            return (Task)method.Invoke(page, new object[] { result, libraries });
        }

        static void SetDatabaseProvider(AnimeDetails page, IDatabaseProvider databaseProvider)
        {
            var property = typeof(BasePage).GetProperty(
                "DatabaseProvider",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(property);
            property.SetValue(page, databaseProvider);
        }

        sealed class StubDatabaseProvider : IDatabaseProvider
        {
            public IList<LibraryEntry> SavedLibraries { get; private set; }

            public ValueTask<User> GetUserAsync() => throw new NotImplementedException();
            public ValueTask SetUserAsync(User value) => throw new NotImplementedException();
            public ValueTask<long?> GetLastFetchedIdAsnyc() => throw new NotImplementedException();
            public ValueTask SetLastFetchedIdAsync(long? value) => throw new NotImplementedException();
            public ValueTask<DateTimeOffset?> GetLastFetchedDateAsnyc() => throw new NotImplementedException();
            public ValueTask SetLastFetchedDateAsync(DateTimeOffset value) => throw new NotImplementedException();
            public ValueTask<IList<LibraryEntry>> GetLibrariesAsync() => throw new NotImplementedException();

            public ValueTask SetLibrariesAsync(IList<LibraryEntry> value)
            {
                SavedLibraries = value;
                return ValueTask.CompletedTask;
            }

            public ValueTask<int?> GetMigrationVersionAsnyc() => throw new NotImplementedException();
            public ValueTask SetMigrationVersionAsync(int value) => throw new NotImplementedException();
            public ValueTask<UserSettings> GetUserSettingsAsync() => throw new NotImplementedException();
            public ValueTask SetUserSettingsAsync(UserSettings value) => throw new NotImplementedException();
            public ValueTask ClearAsync() => throw new NotImplementedException();
        }
    }
}
