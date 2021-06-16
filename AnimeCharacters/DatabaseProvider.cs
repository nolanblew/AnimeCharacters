﻿using AnimeCharacters.Events;
using Blazored.LocalStorage;
using EventAggregator.Blazor;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters
{
    public interface IDatabaseProvider
    {
        ValueTask<User> GetUserAsync();
        ValueTask SetUserAsync(User value);

        ValueTask<long?> GetLastFetchedIdAsnyc();
        ValueTask SetLastFetchedIdAsync(long? value);

        ValueTask<DateTimeOffset?> GetLastFetchedDateAsnyc();
        ValueTask SetLastFetchedDateAsync(DateTimeOffset value);

        ValueTask<IList<LibraryEntry>> GetLibrariesAsync();
        ValueTask SetLibrariesAsync(IList<LibraryEntry> value);

        ValueTask ClearAsync();
    }

    public class DatabaseProvider : IDatabaseProvider
    {
        const string _USER_STORE = "user";
        const string _LAST_FETCHED_ID_STORE = "last_fetched_id";
        const string _LAST_FETCHED_DATE_STORE = "last_fetched_date";
        const string _LIBRARIES_STORE = "libraries";

        public DatabaseProvider(
            ILocalStorageService localStorageService,
            IEventAggregator eventAggregator)
        {
            _localStorageService = localStorageService;
            _eventAggregator = eventAggregator;
        }

        ILocalStorageService _localStorageService;
        IEventAggregator _eventAggregator;

        // User
        public ValueTask<User> GetUserAsync() => _localStorageService.GetItemAsync<User>(_USER_STORE);
        public ValueTask SetUserAsync(User value) => _TriggerEvent(() => _localStorageService.SetItemAsync(_USER_STORE, value));

        // Last Fetched
        public ValueTask<long?> GetLastFetchedIdAsnyc() => _localStorageService.GetItemAsync<long?>(_LAST_FETCHED_ID_STORE);
        public ValueTask SetLastFetchedIdAsync(long? value) => _TriggerEvent(() => _localStorageService.SetItemAsync(_LAST_FETCHED_ID_STORE, value));

        public ValueTask<DateTimeOffset?> GetLastFetchedDateAsnyc() => _localStorageService.GetItemAsync<DateTimeOffset?>(_LAST_FETCHED_DATE_STORE);
        public ValueTask SetLastFetchedDateAsync(DateTimeOffset value) => _TriggerEvent(() => _localStorageService.SetItemAsync(_LAST_FETCHED_DATE_STORE, value));

        // Libraries
        public ValueTask<IList<LibraryEntry>> GetLibrariesAsync() => _localStorageService.GetItemAsync<IList<LibraryEntry>>(_LIBRARIES_STORE);
        public ValueTask SetLibrariesAsync(IList<LibraryEntry> value) => _TriggerEvent(() => _localStorageService.SetItemAsync(_LIBRARIES_STORE, value));

        public ValueTask ClearAsync() => _TriggerEvent(() => _localStorageService.ClearAsync());

        T _TriggerEvent<T>(Func<T> fun)
        {
            var result = fun();

            _eventAggregator.PublishAsync(new DatabaseEvent());

            return result;
        }
    }
}
