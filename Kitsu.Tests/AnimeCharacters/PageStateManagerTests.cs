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

        [TestMethod]
        public void TryGetPageState_WhenCachedStateMatchesType_ReturnsState()
        {
            var manager = new PageStateManager(new NoopEventAggregator());
            var state = new TestPageState("cached page");

            manager.SetPageState("https://example.com/animes/123", state);

            Assert.IsTrue(manager.TryGetPageState<TestPageState>("https://example.com/animes/123", out var cachedState));
            Assert.AreSame(state, cachedState);
        }

        [TestMethod]
        public void SetPageState_WhenMoreThanFourPagesAreCached_EvictsOldestPage()
        {
            var manager = new PageStateManager(new NoopEventAggregator());

            manager.SetPageState("page-1", new TestPageState("one"));
            manager.SetPageState("page-2", new TestPageState("two"));
            manager.SetPageState("page-3", new TestPageState("three"));
            manager.SetPageState("page-4", new TestPageState("four"));
            manager.SetPageState("page-5", new TestPageState("five"));

            Assert.IsFalse(manager.TryGetPageState<TestPageState>("page-1", out _));
            Assert.IsTrue(manager.TryGetPageState<TestPageState>("page-2", out _));
            Assert.IsTrue(manager.TryGetPageState<TestPageState>("page-5", out _));
        }

        [TestMethod]
        public void TryGetPageState_WhenCachedStateHasDifferentType_ReturnsFalse()
        {
            var manager = new PageStateManager(new NoopEventAggregator());

            manager.SetPageState("https://example.com/animes/123", new TestPageState("cached page"));

            Assert.IsFalse(manager.TryGetPageState<string>("https://example.com/animes/123", out _));
        }

        private record TestPageState(string Title);

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
