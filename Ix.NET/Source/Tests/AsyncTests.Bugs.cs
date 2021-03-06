﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
#if !NO_TPL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tests
{
    [TestFixture]
    public partial class AsyncTests
    {
        [SetUp]
        public void InitTests()
        {
            TaskScheduler.UnobservedTaskException += (o, e) =>
            {
            };
        }

        /*
        [Test]
        public void TestPushPopAsync()
        {
            var stack = new Stack<int>();
            var count = 10;

            var observable = Observable.Generate(
              0,
              i => i < count,
              i => i + 1,
              i => i,
              i => TimeSpan.FromMilliseconds(1), // change this to 0 to avoid the problem [1]
              Scheduler.ThreadPool);

            var task = DoSomethingAsync(observable, stack);

            // we give it a timeout so the test can fail instead of hang
            task.Wait(TimeSpan.FromSeconds(2));

            Assert.AreEqual(10, stack.Count);
        }

        private Task DoSomethingAsync(IObservable<int> observable, Stack<int> stack)
        {
            var ae = observable
              .ToAsyncEnumerable()
                //.Do(i => Debug.WriteLine("Bug-fixing side effect: " + i))   // [2]
              .GetEnumerator();

            var tcs = new TaskCompletionSource<object>();

            var a = default(Action);
            a = new Action(() =>
            {
                ae.MoveNext().ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        var i = ae.Current;
                        Debug.WriteLine("Doing something with " + i);
                        Thread.Sleep(50);
                        stack.Push(i);
                        a();
                    }
                    else
                        tcs.TrySetResult(null);
                });
            });

            a();

            return tcs.Task;
        }
        */

        static IEnumerable<int> Xs(Action a)
        {
            try
            {
                var rnd = new Random();

                while (true)
                {
                    yield return rnd.Next(0, 43);
                    Thread.Sleep(rnd.Next(0, 500));
                }
            }
            finally
            {
                a();
            }
        }

        [Test]
        public void CorrectDispose()
        {
            var disposed = new TaskCompletionSource<bool>();

            var xs = new[] { 1, 2, 3 }.WithDispose(() =>
            {
                disposed.TrySetResult(true);
            }).ToAsyncEnumerable();

            var ys = xs.Select(x => x + 1);

            var e = ys.GetEnumerator();
            e.Dispose();

            Assert.IsTrue(disposed.Task.Result);

            Assert.IsFalse(e.MoveNext().Result);
        }

        [Test]
        public void DisposesUponError()
        {
            var disposed = new TaskCompletionSource<bool>();

            var xs = new[] { 1, 2, 3 }.WithDispose(() =>
            {
                disposed.SetResult(true);
            }).ToAsyncEnumerable();

            var ex = new Exception("Bang!");
            var ys = xs.Select(x => { if (x == 1) throw ex; return x; });

            var e = ys.GetEnumerator();
            AssertThrows<Exception>(() => e.MoveNext().Wait());

            Assert.IsTrue(disposed.Task.Result);
        }

        [Test]
        public void CorrectCancel()
        {
            var disposed = new TaskCompletionSource<bool>();

            var xs = new[] { 1, 2, 3 }.WithDispose(() =>
            {
                disposed.SetResult(true);
            }).ToAsyncEnumerable();

            var ys = xs.Select(x => x + 1).Where(x => true);

            var e = ys.GetEnumerator();
            var cts = new CancellationTokenSource();
            var t = e.MoveNext(cts.Token);

            cts.Cancel();

            try
            {
                t.Wait();
            }
            catch
            {
                // Don't care about the outcome; we could have made it to element 1
                // but we could also have cancelled the MoveNext-calling task. Either
                // way, we want to wait for the task to be completed and check that
            }
            finally
            {
                // the cancellation bubbled all the way up to the source to dispose
                // it. This design is chosen because cancelling a MoveNext call leaves
                // the enumerator in an indeterminate state. Further interactions with
                // it should be forbidden.
                Assert.IsTrue(disposed.Task.Result);
            }

            Assert.IsFalse(e.MoveNext().Result);
        }

        [Test]
        public void CanCancelMoveNext()
        {
            var evt = new ManualResetEvent(false);
            var xs = Blocking(evt).ToAsyncEnumerable().Select(x => x).Where(x => true);

            var e = xs.GetEnumerator();
            var cts = new CancellationTokenSource();
            var t = e.MoveNext(cts.Token);

            cts.Cancel();

            try
            {
                t.Wait();
                Assert.Fail();
            }
            catch
            {
                Assert.IsTrue(t.IsCanceled);
            }

            evt.Set();
        }

        static IEnumerable<int> Blocking(ManualResetEvent evt)
        {
            evt.WaitOne();
            yield return 42;
        }

        [Test]
        public void TakeOneFromSelectMany()
        {
            var enumerable = AsyncEnumerable
                .Return(0)
                .SelectMany(_ => AsyncEnumerable.Return("Check"))
                .Take(1)
                .Do(_ => { });

            Assert.AreEqual("Check", enumerable.First().Result);
        }

        [Test]
        public void SelectManyDisposeInvokedOnlyOnce()
        {
            var disposeCounter = new DisposeCounter();

            var result = AsyncEnumerable.Return(1).SelectMany(i => disposeCounter).Select(i => i).ToList().Result;

            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(1, disposeCounter.DisposeCount);
        }

        [Test]
        public void SelectManyInnerDispose()
        {
            var disposes = Enumerable.Range(0, 10).Select(_ => new DisposeCounter()).ToList();

            var result = AsyncEnumerable.Range(0, 10).SelectMany(i => disposes[i]).Select(i => i).ToList().Result;

            Assert.AreEqual(0, result.Count);
            Assert.IsTrue(disposes.All(d => d.DisposeCount == 1));
        }

        private class DisposeCounter : IAsyncEnumerable<object>
        {
            public int DisposeCount { get; private set; }

            public IAsyncEnumerator<object> GetEnumerator()
            {
                return new Enumerator(this);
            }

            private class Enumerator : IAsyncEnumerator<object>
            {
                private readonly DisposeCounter _disposeCounter;

                public Enumerator(DisposeCounter disposeCounter)
                {
                    _disposeCounter = disposeCounter;
                }

                public void Dispose()
                {
                    _disposeCounter.DisposeCount++;
                }

                public Task<bool> MoveNext(CancellationToken _)
                {
                    return Task.Factory.StartNew(() => false);
                }

                public object Current { get; private set; }
            }
        }
    }

    static class MyExt
    {
        public static IEnumerable<T> WithDispose<T>(this IEnumerable<T> source, Action a)
        {
            return EnumerableEx.Create(() =>
            {
                var e = source.GetEnumerator();
                return new Enumerator<T>(e.MoveNext, () => e.Current, () => { e.Dispose(); a(); });
            });
        }

        class Enumerator<T> : IEnumerator<T>
        {
            private readonly Func<bool> _moveNext;
            private readonly Func<T> _current;
            private readonly Action _dispose;

            public Enumerator(Func<bool> moveNext, Func<T> current, Action dispose)
            {
                _moveNext = moveNext;
                _current = current;
                _dispose = dispose;
            }

            public T Current
            {
                get { return _current(); }
            }

            public void Dispose()
            {
                _dispose();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return _moveNext();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

    }
}

#endif
