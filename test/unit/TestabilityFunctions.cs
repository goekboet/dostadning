using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Microsoft.Reactive.Testing;

namespace unit
{
    public static class TestabilityFunctions
    {
        public static ITestableObserver<T> LetRun<T>(
            this TestScheduler s,
            Func<IObservable<T>> es
        ) => s.Start(es, 0, 0, long.MaxValue);

        public static string Show<T>(ITestableObserver<T> es) =>
            string.Join(Environment.NewLine, es.Messages.Select(x => x.ToString()));

        public static IEnumerable<T> GetValues<T>(
            this ITestableObserver<T> o) => o.GetValues(NotificationKind.OnNext)
                .Select(x => x.Value);

        public static (bool, Exception) Errored<T>(
            this ITestableObserver<T> o) => o.GetValues(NotificationKind.OnError)
                .Select(x => (true, x.Exception))
                .SingleOrDefault();

        public static bool Completed<T>(
            this ITestableObserver<T> o) => o.GetValues(NotificationKind.OnCompleted)
                .Select(_ => true)
                .SingleOrDefault();

        public static IEnumerable<Notification<T>> GetValues<T>(
            this ITestableObserver<T> o,
            NotificationKind k) => o.Messages
                .Select(x => x.Value)
                .Where(x => x.Kind == k);

        public static ITestableObservable<T> TestStreamHot<T>(
            this TestScheduler s,
            IEnumerable<(long t, T v)> es)
        {
            return s.CreateHotObservable<T>(
                es
                .Select(e => ReactiveTest.OnNext(e.t, e.v))
                .Concat(new[] { ReactiveTest.OnCompleted<T>(es.Max(x => x.t)) })
                .ToArray());
        }
    }
}