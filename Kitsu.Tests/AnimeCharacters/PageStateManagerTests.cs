using AnimeCharacters;
using EventAggregator.Blazor;
using Kitsu.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class PageStateManagerTests
    {
        [TestMethod]
        public void GetSelectedLibraryEntry_WhenEntryMatchesKitsuId_ReturnsEntry()
        {
            var manager = new PageStateManager(new NoopEventAggregator());
            var entry = new LibraryEntry
            {
                Anime = new Anime
                {
                    KitsuId = "123",
                    Title = "Test Anime"
                }
            };

            manager.SetSelectedLibraryEntry(entry);

            Assert.AreSame(entry, manager.GetSelectedLibraryEntry("123"));
        }

        [TestMethod]
        public void GetSelectedLibraryEntry_WhenEntryDoesNotMatchKitsuId_ReturnsNull()
        {
            var manager = new PageStateManager(new NoopEventAggregator());
            var entry = new LibraryEntry
            {
                Anime = new Anime
                {
                    KitsuId = "123",
                    Title = "Test Anime"
                }
            };

            manager.SetSelectedLibraryEntry(entry);

            Assert.IsNull(manager.GetSelectedLibraryEntry("456"));
        }

        private class NoopEventAggregator : IEventAggregator
        {
            public void Subscribe(object subscriber)
            {
            }

            public void Unsubscribe(object subscriber)
            {
            }

            public Task PublishAsync(object message) => Task.CompletedTask;
        }
    }
}
