using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace dostadning.domain.lookups
{
    public struct WatchComparison
    {
        public WatchComparison(DateTime other, DateTimeOffset us)
        {
            Other = other;
            Us = us;
        }
        public DateTime Other { get; }
        public DateTimeOffset Us { get; }

        static string n => Environment.NewLine;
        public override string ToString() => 
            $"Tradera: {Other} kind: {Other.Kind} {n}" +
            $"Us: {Us} timezone: {TimeZoneInfo.Local}";
    }

    public static class LookupFeatures
    {
        public static IObservable<WatchComparison> CompareWatches(
            ILookupCalls soap
        ) => soap.ServerTime().Select(x => new WatchComparison(x, DateTimeOffset.Now));
        public static IObservable<IEnumerable<Lookup>> GetLookups(
            ILookupCalls soap) => Observable.Merge(
            soap.GetItemRequestLookups(),
            soap.GetAcceptedBidderTypes(),
            soap.GetExpoItemTypes(),
            soap.GetItemTypes())
            .Aggregate(Enumerable.Concat);
    }
}