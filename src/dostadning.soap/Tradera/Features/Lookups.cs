using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using dostadning.domain.features;
using dostadning.domain.result;
using dostadning.domain.service.tradera;
using dostadning.soap.tradera.publicservice;
using static dostadning.soap.tradera.publicservice.PublicServiceSoapClient;

namespace dostadning.soap.tradera.feature
{
    public sealed class GetLookups : 
        SoapClient<PublicServiceSoapClient>,
        ILookupCalls
    {
        public static ILookupCalls Init(AppIdentity app) =>
            new GetLookups(new PublicServiceSoapClient(EndpointConfiguration.PublicServiceSoap12), app);

        public GetLookups(
            PublicServiceSoapClient c, 
            AppIdentity app) : base(c, app) {}

        static Error ResultWasNull => new Error("Result points to null.");

        static IEnumerable<Constant> ToConstant(IdDescriptionPair p) => 
            p is IdDescriptionPair
            ? new [] {new Constant(p.Id, $"{p.Description} {p.Value ?? string.Empty}")}
            : Enumerable.Empty<Constant>();

        static Constant VATConstant(int v) => new Constant(v, $"{v}% VAT");
        static IEnumerable<Lookup> MapToOur(
                GetItemFieldValuesResponse response) =>  
            response.GetItemFieldValuesResult is ItemFieldsResponse r
                ? new []
                    {
                        new Lookup("VAT", r.VAT.Select(VATConstant)),
                        new Lookup("ItemAttributes", r.ItemAttributes.SelectMany(ToConstant)),
                        new Lookup("PaymentTypes", r.PaymentTypes.SelectMany(ToConstant)),
                        new Lookup("ShippingTypes", r.ShippingTypes.SelectMany(ToConstant))
                    }
                : throw ResultWasNull;   

        public IObservable<IEnumerable<Lookup>> GetItemRequestLookups() =>
            Observable.FromAsync(() => Client.GetItemFieldValuesAsync(AuthN, Conf))
                .Select(MapToOur)
                .Errorcontext("ItemRequestType");

        public Func<IdDescriptionPair[], IEnumerable<Lookup>> ToLookup(string k) => r =>
            r != null
                ? new[] { new Lookup(k, r.SelectMany(ToConstant))}
                : throw ResultWasNull;

        
        public IObservable<IEnumerable<Lookup>> GetAcceptedBidderTypes() =>
            Observable.FromAsync(() => Client.GetAcceptedBidderTypesAsync(AuthN, Conf))
                .Select(x => x.GetAcceptedBidderTypesResult)
                .Select(ToLookup("AcceptedBidderType"))
                .Errorcontext("AcceptedBidderType");

        public IObservable<IEnumerable<Lookup>> GetExpoItemTypes() => 
            Observable.FromAsync(() => Client.GetExpoItemTypesAsync(AuthN, Conf))
                .Select(x => x.GetExpoItemTypesResult)
                .Select(ToLookup("ExpoItemTypes"))
                .Errorcontext("ExpoItemTypes");

        public IObservable<IEnumerable<Lookup>> GetItemTypes() =>
            Observable.FromAsync(() => Client.GetItemTypesAsync(AuthN, Conf))
                .Select(x => x.GetItemTypesResult)
                .Select(ToLookup("ItemTypes"))
                .Errorcontext("ItemTypes");

        public IObservable<DateTime> ServerTime() =>
            Observable.FromAsync(() => Client.GetOfficalTimeAsync(AuthN, Conf))
            .Select(x => x.GetOfficalTimeResult);
    }

    public static class Extensions
    {
        public static IObservable<IEnumerable<Lookup>> Errorcontext(
            this IObservable<IEnumerable<Lookup>> ls,
            string context) => ls
                .Catch<IEnumerable<Lookup>, Error>(e => 
                    Observable.Throw<IEnumerable<Lookup>>(new Error(context, e)));
    }
}