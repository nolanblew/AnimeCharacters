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
    }

    public class PageStateManager : IPageStateManager
    {
        public PageStateManager(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        readonly IEventAggregator _eventAggregator;

        readonly Stack<string> _pageHistory = new();

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

        public void Clear()
        {
            _pageHistory.Clear();
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
