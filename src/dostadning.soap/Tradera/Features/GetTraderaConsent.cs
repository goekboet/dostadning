using System;
using System.Threading.Tasks;
using dostadning.domain.service.tradera;
using dostadning.domain.result;
using dostadning.soap.tradera.publicservice;
using static dostadning.soap.tradera.publicservice.PublicServiceSoapClient;

namespace dostadning.soap.tradera.feature
{
    public sealed class GetTraderaConsent : IGetTraderaConsentCalls
    {
        public static IGetTraderaConsentCalls Init(AppIdentity app) =>
            new GetTraderaConsent(new PublicServiceSoapClient(EndpointConfiguration.PublicServiceSoap12), app);
        PublicServiceSoapClient Client { get; }
        AppIdentity App { get; }

        AuthenticationHeader Auth => new AuthenticationHeader
        {
            AppId = App.Id,
            AppKey = App.Key
        };

        static ConfigurationHeader Conf => new ConfigurationHeader();
        private GetTraderaConsent(
            PublicServiceSoapClient c,
            AppIdentity app)
        { Client = c; App = app; }

        static Error TraderaAliasNotFound { get; } = new DomainError("traderauser.notfound");
        public async Task<Either<int>> Identify(string alias)
        {
            var r = await Client.GetUserByAliasAsync(Auth, Conf, alias);

            return r.GetUserByAliasResult != null
                ? new Either<int>(r.GetUserByAliasResult.Id)
                : new Either<int>(TraderaAliasNotFound);
        }

        static Error NoTokenAvailiable { get; } = new DomainError("traderauser.no_token_availiable");
        public async Task<Either<(string token, DateTimeOffset exp)>> FetchToken(int traderaUserId, string requestId)
        {
            var r = await Client.FetchTokenAsync(Auth, Conf, traderaUserId, requestId);

            return r.FetchTokenResult != null
                ? new Either<(string token, DateTimeOffset exp)>((r.FetchTokenResult.AuthToken, r.FetchTokenResult.HardExpirationTime))
                : new Either<(string token, DateTimeOffset exp)>(NoTokenAvailiable);
        }
    }
}