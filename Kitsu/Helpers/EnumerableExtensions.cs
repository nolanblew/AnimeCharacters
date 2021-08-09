using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsu.Helpers
{
    public static class EnumerableExtensions
    {

        public static (IEnumerable<TList> Added, IEnumerable<TList> Updated, IEnumerable<TList> Deleted) GetDelta<TList, TComparer>(IEnumerable<TList> list) =>
            GetDelta(list, l => l);

        public static (IEnumerable<TList> Added, IEnumerable<TList> Updated, IEnumerable<TList> Deleted) GetDelta<TList, TComparer>(IEnumerable<TList> list, Func<TList, TComparer> comparer)
        {

        }
    }
}
