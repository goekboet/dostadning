using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using dostadning.domain.service.tradera;

namespace dostadning.domain.features
{
    public struct WatchComparison
    {
        public WatchComparison(DateTime tradera, DateTimeOffset us)
        {
            Tradera = tradera;
            Us = us;
        }
        public DateTime Tradera { get; }
        public DateTimeOffset Us { get; }

        static string n => Environment.NewLine;
        public override string ToString() => 
            $"Tradera: {Tradera} kind: {Tradera.Kind} {n}" +
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
            soap.GetExpoItemTypes())
            .Aggregate(Enumerable.Concat);
    }
}