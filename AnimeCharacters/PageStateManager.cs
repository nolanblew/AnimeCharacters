using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnimeCharacters
{
    public interface IPageStateManager : INotifyPropertyChanged
    {
        bool CanGoBack { get; }
        string CurrentPage { get; }

        void Add(string url);
        void Clear();
        string GetLastPage();
        string GoBack();
    }

    public class PageStateManager : IPageStateManager, INotifyPropertyChanged
    {
        readonly Stack<string> _pageHistory = new();

        public string CurrentPage { get; private set; }

        public void Add(string url)
        {
            if (url == CurrentPage) { return; }

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

        public void Clear()
        {
            _pageHistory.Clear();
            CurrentPage = null;
            _NotifyPropertyChanges();
        }

        void _NotifyPropertyChanges()
        {
            _OnPropertyChanged(nameof(CurrentPage));
            _OnPropertyChanged(nameof(CanGoBack));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void _OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
