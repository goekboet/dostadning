using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.ServiceModel;
using dostadning.domain;
using dostadning.domain.auction;
using dostadning.domain.seller;
using dostadning.soap.tradera.restrictedservice;
using static dostadning.soap.tradera.restrictedservice.RestrictedServiceSoapClient;
using Auction = dostadning.soap.tradera.feature.AuctionSoapCalls;

namespace dostadning.soap.tradera.feature
{
    public static class AuctionSoapCalls
    {
        public static IAuctionProcedures Init(AppIdentity app) =>
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
            UserId = c.Id.TraderaUser,
            Token = c.Token
        };

        static Error ResultWasNull => new Error("Result points to null.");

        public static AuctionHandle ToDomain(string lotId, AddItemResponse r) =>
            r.AddItemResult is QueuedRequestResponse x
                ? new AuctionHandle(lotId, x.ItemId, x.RequestId)
                : throw ResultWasNull;
        
        
        public static string ToDomain(this ResultCode c, int rId)
        {
            if (c.HasFlag(ResultCode.TryAgain))
                return Status.Retry;
            else if (c.HasFlag(ResultCode.Ok))
                return Status.Done;
            else if (c.HasFlag(ResultCode.Error))
                return Status.Error;
            else
                return Status.Pending;
        }
        
        public static Status ToDomain(this RequestResult r) => r != null
            ? Status.Create(r.RequestId, r.ResultCode.ToDomain(r.RequestId), $"{r.Message} ({r.ResultCode})")
            : null;
        public static IEnumerable<Status> ToDomain(
            this GetRequestResultsResponse r) =>
            r.GetRequestResultsResult != null
            ? r.GetRequestResultsResult
                .Select(x => x.ToDomain())
                .Where(x => x != null) 
            : Enumerable.Empty<Status>();
    }

    public sealed class InventoryCalls :
        SoapClient<RestrictedServiceSoapClient>,
        IAuctionProcedures
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
        public Error TraderaError(FaultException e, Consent c, string args) => 
        new Error($"Call to tradera restricted service faulted:{n}" + 
                  $"Input:{n}" +
                  $"Consent: {c}{n}" +
                  $"Args: {args}{n}" +
                  $"remote msg: {e.Message}", e);

        public IObservable<AuctionHandle> AddLot(
            Consent c,
            Lot l) => Observable
            .FromAsync(() => Client.AddItemAsync(
                AuthenticationHeader: AuthNP,
                AuthorizationHeader: Auction.MapFrom(c),
                ConfigurationHeader: ConfP,
                itemRequest: Auction.MapFrom(l)))
            .Select(x => Auction.ToDomain(l.Id, x))
            .Catch<AuctionHandle, FaultException>(
                e => Observable.Throw<AuctionHandle>(TraderaError(e, c, l.ToString())));

        ImageFormat F(Image i)
        {
            switch (i.Type.MIME.Split('/').Last())
            {
                case "gif": return ImageFormat.Gif;
                case "jpeg": return ImageFormat.Jpeg;
                case "png": return ImageFormat.Png;
                default: throw new Error($"No map to ImageFormat from {i.Type}");
            }
        }
        public IObservable<Unit> AddImage(
            Consent c, 
            Image i, 
            int id) => 
            Observable.FromAsync(() => Client.AddItemImageAsync(
                AuthenticationHeader: AuthNP,
                AuthorizationHeader: Auction.MapFrom(c),
                ConfigurationHeader: ConfP,
                requestId: id,
                imageData: i.Bytes,
                imageFormat: F(i),
                hasMega: false))
            .Select(_ => Unit.Default)
            .Catch<Unit, FaultException>(
                e => Observable.Throw<Unit>(TraderaError(e, c, AddImageFault(i, id))));

        string AddImageFault(Image i, int id) => 
            $"RequestId: {id} Image: {i}";

        public IObservable<Unit> Commit(Consent c, int id) =>
            Observable.FromAsync(() => Client.AddItemCommitAsync(
                AuthenticationHeader: AuthNP,
                AuthorizationHeader: Auction.MapFrom(c),
                ConfigurationHeader: ConfP,
                requestId: id))
            .Select(_ => Unit.Default)
            .Catch<Unit, FaultException>(
                e => Observable.Throw<Unit>(TraderaError(e, c, CommitFault(id))));

        string CommitFault(int id) => 
            $"RequestId: {id}";

        public IObservable<IEnumerable<Status>> GetResult(
            Consent c, 
            int[] ids) =>
            Observable.FromAsync(() => Client.GetRequestResultsAsync(
                AuthenticationHeader: AuthNP,
                AuthorizationHeader: Auction.MapFrom(c),
                ConfigurationHeader: ConfP,
                requestIds: ids))
            .Select(x => x.ToDomain())
            .Catch<IEnumerable<Status>, FaultException>(
                e => Observable.Throw<IEnumerable<Status>>(TraderaError(e, c, GetResultFault(ids))));
        string GetResultFault(int[] ids) => string.Join(", ", ids.Select(x => x.ToString()));
    }
}