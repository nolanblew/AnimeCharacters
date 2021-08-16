using System;
using System.Collections.Generic;

namespace Kitsu.Helpers
{
    public static class ArrayExtensions
    {
        public static T[] Slice<T>(this T[] source, int index, int length)
        {
            var maxLength = Math.Min(source.Length - index, length);
            T[] slice = new T[maxLength];
            Array.Copy(source, index, slice, 0, maxLength);
            return slice;
        }

        public static IEnumerable<T[]> Chunk<T>(this T[] source, int chunkSize)
        {
            for (int i = 0; i < source.Length; i += chunkSize)
            {
                yield return source.Slice(i, chunkSize);
            }
        }
    }
}
