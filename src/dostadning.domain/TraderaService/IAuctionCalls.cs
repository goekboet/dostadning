using System;
using System.Collections.Generic;
using dostadning.domain.features;
using dostadning.domain.result;

namespace dostadning.domain.service.tradera
{
    public class Lot
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int[] ItemAttributes { get; set; }

        public int Duration { get; set; }
        public int Restarts { get; set; }
        public int StartPrice { get; set; }
        public int ReservePrice { get; set; }
        public int BuyItNowPrice { get; set; }
        public int VAT { get; set; }
        public int AcceptedBidderId { get; set; }

        public int[] PaymentOptionIds { get; set; }
        public string ShippingCondition { get; set; }
        public string PaymentCondition { get; set; }

        public override string ToString() => "The request in json format.";
    }

    public interface IAuction
    {
        IObservable<AuctionHandle> AddTestItem(Consent c, Lot l);
    }
}
