﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Tests
{
    public partial class Tests
    {
        [Test]
        public void IsEmtpy_Arguments()
        {
            AssertThrows<ArgumentNullException>(() => EnumerableEx.IsEmpty<int>(null));
        }

        [Test]
        public void IsEmpty_Empty()
        {
            Assert.IsTrue(Enumerable.Empty<int>().IsEmpty());
        }

        [Test]
        public void IsEmpty_NonEmpty()
        {
            Assert.IsFalse(new[] { 1 }.IsEmpty());
        }

        [Test]
        public void Min_Arguments()
        {
            AssertThrows<ArgumentNullException>(() => EnumerableEx.Min(null, Comparer<int>.Default));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.Min(new[] { 1 }, null));
        }

        [Test]
        public void Min()
        {
            Assert.AreEqual(3, new[] { 5, 3, 7 }.Min(new Mod3Comparer()));
        }

        class Mod3Comparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return Comparer<int>.Default.Compare(x % 3, y % 3);
            }
        }

        [Test]
        public void MinBy_Arguments()
        {
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MinBy(null, (int x) => x));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MinBy(new[] { 1 }, default(Func<int, int>)));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MinBy(null, (int x) => x, Comparer<int>.Default));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MinBy(new[] { 1 }, default(Func<int, int>), Comparer<int>.Default));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MinBy(new[] { 1 }, (int x) => x, null));
        }

        [Test]
        public void MinBy()
        {
            var res = new[] { 2, 5, 0, 7, 4, 3, 6, 2, 1 }.MinBy(x => x % 3);
            Assert.IsTrue(res.SequenceEqual(new[] { 0, 3, 6 }));
        }

        [Test]
        public void MinBy_Empty()
        {
            AssertThrows<InvalidOperationException>(() => Enumerable.Empty<int>().MinBy(x => x));
        }

        [Test]
        public void Max_Arguments()
        {
            AssertThrows<ArgumentNullException>(() => EnumerableEx.Max(null, Comparer<int>.Default));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.Max(new[] { 1 }, null));
        }

        [Test]
        public void Max()
        {
            Assert.AreEqual(5, new[] { 2, 5, 3, 7 }.Max(new Mod7Comparer()));
        }

        class Mod7Comparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return Comparer<int>.Default.Compare(x % 7, y % 7);
            }
        }

        [Test]
        public void MaxBy_Arguments()
        {
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MaxBy(null, (int x) => x));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MaxBy(new[] { 1 }, default(Func<int, int>)));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MaxBy(null, (int x) => x, Comparer<int>.Default));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MaxBy(new[] { 1 }, default(Func<int, int>), Comparer<int>.Default));
            AssertThrows<ArgumentNullException>(() => EnumerableEx.MaxBy(new[] { 1 }, (int x) => x, null));
        }

        [Test]
        public void MaxBy()
        {
            var res = new[] { 2, 5, 0, 7, 4, 3, 6, 2, 1 }.MaxBy(x => x % 3);
            Assert.IsTrue(res.SequenceEqual(new[] { 2, 5, 2 }));
        }

        [Test]
        public void MaxBy_Empty()
        {
            AssertThrows<InvalidOperationException>(() => Enumerable.Empty<int>().MaxBy(x => x));
        }
    }
}
