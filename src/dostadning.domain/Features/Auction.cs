using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using dostadning.domain.result;
using dostadning.domain.service.tradera;

namespace dostadning.domain.features
{
    public struct AuctionHandle
    {
        public AuctionHandle(int iId, int rId)
        {
            ItemId = iId;
            RequestId = rId;
        }

        public int ItemId { get; }
        public int RequestId { get; }

        public override string ToString() =>
        $"ItemId: {ItemId} RequestId: {RequestId}";
    }
    public static class AuctionFeature
    {
        public static IObservable<AuctionHandle> CreateAuction(
            IAuction soap,
            Consent c,
            Lot l) => soap.AddTestItem(c, l);

    }
}