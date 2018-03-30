using System;
using System.Linq;
using System.Reactive.Linq;
using System.ServiceModel;
using dostadning.domain.features;
using dostadning.domain.result;
using dostadning.domain.service.tradera;
using dostadning.soap.tradera.restrictedservice;
using static dostadning.soap.tradera.restrictedservice.RestrictedServiceSoapClient;
using Auction = dostadning.soap.tradera.feature.AuctionSoapCalls;

namespace dostadning.soap.tradera.feature
{
    public static class AuctionSoapCalls
    {
        public static IAuction Init(AppIdentity app) =>
            new InventoryCalls(new RestrictedServiceSoapClient(
                EndpointConfiguration.RestrictedServiceSoap12), app);

        static ItemShipping Standard { get; } = new ItemShipping
        {
            ShippingOptionId = 1,
            Cost = 100
        };

        public static ItemRequest MapFrom(Lot a) =>
        new ItemRequest
        {
            Title = a.Title,
            CategoryId = 344481,
            Duration = a.Duration,
            Restarts = a.Restarts,
            StartPrice = a.StartPrice,
            ReservePrice = a.ReservePrice,
            BuyItNowPrice = a.BuyItNowPrice,
            Description = a.Description,
            PaymentOptionIds = a.PaymentOptionIds,
            ShippingOptions = new[] { Standard },
            AcceptedBidderId = a.AcceptedBidderId,
            ItemAttributes = a.ItemAttributes,
            ItemType = 1,
            AutoCommit = false,
            VAT = a.VAT,
            ShippingCondition = a.ShippingCondition,
            PaymentCondition = a.PaymentCondition
        };

        public static AuthorizationHeader MapFrom(Consent c) =>
        new AuthorizationHeader
        {
            UserId = c.Id,
            Token = c.Token
        };

        static Error ResultWasNull => new Error("Result points to null.");

        public static AuctionHandle ToDomain(AddItemResponse r) =>
            r.AddItemResult is QueuedRequestResponse x
                ? new AuctionHandle(x.ItemId, x.RequestId)
                : throw ResultWasNull;
    }

    public sealed class InventoryCalls :
        SoapClient<RestrictedServiceSoapClient>,
        IAuction
    {
        public InventoryCalls(
            RestrictedServiceSoapClient c,
            AppIdentity app) : base(c, app) { }

        public AuthenticationHeader AuthNP => new AuthenticationHeader
        {
            AppId = AuthN.AppId,
            AppKey = AuthN.AppKey
        };

        public ConfigurationHeader ConfP => new ConfigurationHeader { };

        static string n => Environment.NewLine;
        public Error TraderaError(FaultException e, Consent c, Lot l) => 
        new Error($"Call to tradera restricted service faulted:{n}" + 
                  $"Input:{n}" +
                  $"Consent: {c}{n}" +
                  $"lot: {l}{n}" +
                  $"remote msg: {e.Message}", e);

        public IObservable<AuctionHandle> AddTestItem(
            Consent c,
            Lot l) => Observable
            .FromAsync(() => Client.AddItemAsync(
                AuthenticationHeader: AuthNP,
                AuthorizationHeader: Auction.MapFrom(c),
                ConfigurationHeader: ConfP,
                itemRequest: Auction.MapFrom(l)))
            .Select(Auction.ToDomain)
            .Catch<AuctionHandle, FaultException>(
                e => Observable.Throw<AuctionHandle>(TraderaError(e, c, l)));

    }
}