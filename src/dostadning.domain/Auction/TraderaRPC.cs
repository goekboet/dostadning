using System;
using System.Collections.Generic;
using System.Reactive;
using dostadning.domain.seller;

namespace dostadning.domain.auction
{
    public interface IAuctionProcedures
    {
        IObservable<AuctionHandle> AddLot(Consent c, Lot l);
        IObservable<Unit> AddImage(Consent c, Image i, int requestId);
        IObservable<Unit> Commit(Consent c, int requestId);
        IObservable<IEnumerable<Update>> GetResult(Consent c, int[] requestIds);
    }
}
