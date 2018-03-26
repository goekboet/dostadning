using System;
using System.Threading.Tasks;
using dostadning.domain.service.tradera;
using dostadning.domain.result;
using dostadning.soap.tradera.publicservice;
using static dostadning.soap.tradera.publicservice.PublicServiceSoapClient;
using System.Reactive.Linq;

using Our = dostadning.domain.service.tradera;
using Their = dostadning.soap.tradera.publicservice;
using dostadning.domain.features;

namespace dostadning.soap.tradera.feature
{
    public sealed class GetAuthorization : 
        SoapClient<PublicServiceSoapClient>,
        IAuthorizationCalls
    {
        public static IAuthorizationCalls Init(AppIdentity app) =>
            new GetAuthorization(new PublicServiceSoapClient(EndpointConfiguration.PublicServiceSoap12), app);
        
        private GetAuthorization(
            PublicServiceSoapClient c,
            AppIdentity app) : base(c, app) {} 

        static IObservable<int> GetId(GetUserByAliasResponse r) => 
            r.GetUserByAliasResult != null
                ? Observable.Return(r.GetUserByAliasResult.Id)
                : Observable.Throw<int>(
                    new Error("No traderauser with that alias according to tradera public service."));

        public IObservable<int> Identify(string alias) => 
            from r in Observable.FromAsync(() => Client.GetUserByAliasAsync(Auth, Conf, alias))
            from id in GetId(r)
            select id;

        static Our.Token MapToOur(Their.Token t) => new Our.Token(
            id: t.AuthToken,
            exp: (DateTimeOffset)t.HardExpirationTime);

        static IObservable<Our.Token> Token(
            FetchTokenResponse r) => r.FetchTokenResult is Their.Token t
                ? Observable.Return<Our.Token>(MapToOur(t))
                : Observable.Throw<Our.Token>(new Error());
        public IObservable<Our.Token> FetchToken(
            int id, 
            string consentId) => Observable
                .FromAsync(() => Client.FetchTokenAsync(Auth, Conf, id, consentId))
                .SelectMany(Token)
                .Catch<Our.Token, Error>(_ => Observable.Throw<Our.Token>(
                        new Error($"TraderauserId {id} has no token for consentId {consentId}")))
                .Catch<Our.Token, Exception>(e => Observable.Throw<Our.Token>(
                        new Error("Error communication with public service", e)));
    }
}