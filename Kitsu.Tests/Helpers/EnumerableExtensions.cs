using Kitsu.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Kitsu.Tests.Helpers
{
    [TestClass]
    public class EnumerableExtensions
    {
        [TestMethod]
        public void TestGetDelta()
        {
            var oldItem1 = new DeltaTest(1, "1");
            var oldItem2 = new DeltaTest(2, "2");
            var oldItem3 = new DeltaTest(3, "3");
            var oldItem4 = new DeltaTest(4, "Four");
            var newItem3 = new DeltaTest(3, "Three");
            var newItem4 = new DeltaTest(4, "Four");
            var newItem5 = new DeltaTest(5, "Five");
            var oldList = new List<DeltaTest> { oldItem1, oldItem2, oldItem3, oldItem4 };
            var newList = new List<DeltaTest> { newItem3, newItem4, newItem5 };

            var expectedAddedItems = new List<DeltaTest> { newItem5 };
            var expectedUpdatedItems = new List<DeltaTest> { newItem3 };
            var expectedRemovedItems = new List<DeltaTest> { oldItem1, oldItem2 };

            var delta = oldList.GetDelta(newList, l => l.Id, DeltaTestComparer.Default);

            CollectionAssert.AreEquivalent(expectedAddedItems, delta.Added.ToList());
            CollectionAssert.AreEquivalent(expectedUpdatedItems, delta.Updated.ToList());
            CollectionAssert.AreEquivalent(expectedRemovedItems, delta.Deleted.ToList());
        }

        class DeltaTest
        {
            public DeltaTest() { }
            public DeltaTest(int id, string data)
            {
                Id = id;
                Data = data;
            }

            public int Id { get; set; }
            public string Data { get; set; }
        }

        class DeltaTestComparer : IEqualityComparer<DeltaTest>
        {
            internal static DeltaTestComparer Default { get; } = new DeltaTestComparer();

            public bool Equals(DeltaTest x, DeltaTest y)
            {
                if (x == null && y == null) { return true; }
                if (x == null || y == null) { return false; }

                return
                    x.Id == y.Id &&
                    x.Data == y.Data;
            }

            public int GetHashCode([DisallowNull] DeltaTest obj) =>
                HashCode.Combine(obj.Id, obj.Data);
        }
    }
}
