using EventAggregator.Blazor;
using System.Collections.Generic;
using Kitsu.Models;
using System.Linq;

namespace AnimeCharacters
{
    public interface IPageStateManager
    {
        bool CanGoBack { get; }
        string CurrentPage { get; }

        void Add(string url);
        void Clear();
        LibraryEntry GetSelectedLibraryEntry(string kitsuId);
        string GetLastPage();
        string GoBack();
        void SetSelectedLibraryEntry(LibraryEntry libraryEntry);
        void SetPageState<TState>(string url, TState state) where TState : class;
        bool TryGetPageState<TState>(string url, out TState state) where TState : class;
    }

    public class PageStateManager : IPageStateManager
    {
        const int _MAX_CACHED_PAGE_STATES = 4;

        public PageStateManager(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        readonly IEventAggregator _eventAggregator;

        readonly Stack<string> _pageHistory = new();

        readonly Dictionary<string, object> _pageStates = new();

        readonly LinkedList<string> _pageStateUrls = new();

        LibraryEntry _selectedLibraryEntry;

        public string CurrentPage { get; private set; }

        public void Add(string url)
        {
            if (url == null || url == CurrentPage) { return; }

            if (!string.IsNullOrWhiteSpace(CurrentPage))
            {
                _pageHistory.Push(CurrentPage);
            }

            CurrentPage = url;
            _NotifyPropertyChanges();
        }

        public bool CanGoBack => _pageHistory.Any();

        public string GoBack()
        {
            if (!CanGoBack) { return CurrentPage; }

            CurrentPage = _pageHistory.Pop();
            _NotifyPropertyChanges();
            return CurrentPage;
        }

        public string GetLastPage()
        {
            if (!CanGoBack) { return null; }
            return _pageHistory.Peek();
        }

        public void SetSelectedLibraryEntry(LibraryEntry libraryEntry)
        {
            _selectedLibraryEntry = libraryEntry;
        }

        public LibraryEntry GetSelectedLibraryEntry(string kitsuId)
        {
            if (string.IsNullOrWhiteSpace(kitsuId)
                || _selectedLibraryEntry?.Anime?.KitsuId != kitsuId)
            {
                return null;
            }

            return _selectedLibraryEntry;
        }

        public void SetPageState<TState>(string url, TState state) where TState : class
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            _pageStateUrls.Remove(url);

            if (state == null)
            {
                _pageStates.Remove(url);
                return;
            }

            _pageStates[url] = state;
            _pageStateUrls.AddLast(url);

            while (_pageStateUrls.Count > _MAX_CACHED_PAGE_STATES)
            {
                var oldestUrl = _pageStateUrls.First.Value;
                _pageStateUrls.RemoveFirst();
                _pageStates.Remove(oldestUrl);
            }
        }

        public bool TryGetPageState<TState>(string url, out TState state) where TState : class
        {
            state = null;

            if (string.IsNullOrWhiteSpace(url)
                || !_pageStates.TryGetValue(url, out var cachedState)
                || cachedState is not TState typedState)
            {
                return false;
            }

            _pageStateUrls.Remove(url);
            _pageStateUrls.AddLast(url);
            state = typedState;
            return true;
        }

        public void Clear()
        {
            _pageHistory.Clear();
            _pageStates.Clear();
            _pageStateUrls.Clear();
            CurrentPage = null;
            _selectedLibraryEntry = null;
            _NotifyPropertyChanges();
        }

        void _NotifyPropertyChanges()
        {
            _eventAggregator.PublishAsync(new Events.PageStateManagerEvent());
        }
    }
}
