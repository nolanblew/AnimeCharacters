using Kitsu.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kitsu.Helpers
{
    public static class EnumerableExtensions
    {

        public static (IEnumerable<TList> Added, IEnumerable<TList> Updated, IEnumerable<TList> Deleted) GetDelta<TList, TComparer>(this IEnumerable<TList> oldList, IEnumerable<TList> newList) =>
            GetDelta(oldList, newList, l => l, EqualityComparer<TList>.Default);

        public static (IEnumerable<TList> Added, IEnumerable<TList> Updated, IEnumerable<TList> Deleted) GetDelta<TList, TComparer>(this IEnumerable<TList> oldList, IEnumerable<TList> newList, Func<TList, TComparer> idFunc) =>
            GetDelta(oldList, newList, idFunc, EqualityComparer<TList>.Default);

        public static (IEnumerable<TList> Added, IEnumerable<TList> Updated, IEnumerable<TList> Deleted) GetDelta<TList, TComparer>(this IEnumerable<TList> oldList, IEnumerable<TList> newList, Func<TList, TComparer> idFunc, IEqualityComparer<TList> updatedComparer)
        {
            var comparer = ProjectionEqualityComparer.Create(idFunc);

            var addedItems = newList.Except(oldList, comparer);
            var removedItems = oldList.Except(newList, comparer);

            var oldDictionaryList = oldList.Except(removedItems);
            var newDictionaryList = newList.Except(addedItems);

            var updatedItems = newDictionaryList.Except(oldDictionaryList, updatedComparer);

            return (addedItems, updatedItems, removedItems);
        }
    }
}
