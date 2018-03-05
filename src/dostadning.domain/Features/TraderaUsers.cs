using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dostadning.domain.ourdata;
using dostadning.domain.result;
using dostadning.domain.service.tradera;
using Dto = dostadning.domain.features;
using Our = dostadning.domain.ourdata;

namespace dostadning.domain.features
{
    public static class AppUserExtensions
    {
        public static TraderaConsent ConsentForAlias(
            this Account u,
            string alias) =>
            u.TraderaUsers.Single(x => x.Alias == alias).Consent;

        public static TraderaUser GetTraderaUser(
            this Account u,
            string alias) =>
            u.TraderaUsers.Single(x => x.Alias.Equals(alias));
    }

    public static class GetTraderaConsentFeature
    {

        public static Task<Either<int>> PairWithId(
            IGetTraderaConsentCalls soap,
            string alias) => soap.Identify(alias);

        static Error TraderaUserAlreadyAdded { get; } = new DomainError("dostadning.domain.traderauserAlreadyAdded");
        public static async Task<Either<string>> AddTraderaUser(
            IRepository<Account, Guid> users,
            IGetTraderaConsentCalls soap,
            string claim,
            string alias)
        {
            var user = await users.Find(Guid.Parse(claim));
            if (!user.IsError)
            {
                var u = user.Result;
                if (u.TraderaUsers.Count(x => x.Alias == alias) != 0)
                    return new Either<string>(TraderaUserAlreadyAdded);
                else
                {
                    var traderaId = await PairWithId(soap, alias);
                    if (traderaId.IsError)
                        return new Either<string>(traderaId.Error);
                    else
                    {
                        var consent = new Our.TraderaConsent() { Id = Guid.NewGuid() };
                        var traderaUser = new Our.TraderaUser
                        {
                            Id = traderaId.Result,
                            Alias = alias,
                            Consent = consent
                        };

                        u.TraderaUsers.Add(traderaUser);
                        var c = await users.Commit();

                        if (c.IsError) return new Either<string>(c.Error);
                        else return new Either<string>(consent.Id.ToString());
                    }
                }
            }
            else
                return new Either<string>(user.Error);
        }

        public static async Task<Either<DateTimeOffset>> FetchConsent(
            IRepository<Account, Guid> users,
            IGetTraderaConsentCalls soap,
            string claim,
            string alias)
        {
            var user = await users.Find(Guid.Parse(claim));
            if (!user.IsError)
            {
                var u = user.Result.GetTraderaUser(alias);
                var c = await soap.FetchToken(u.Id, u.Consent.Id.ToString());

                if (!c.IsError)
                {
                    u.Consent.Expires = c.Result.exp;
                    u.Consent.Token = c.Result.token;

                    var commit = await users.Commit();

                    if (!commit.IsError)
                        return new Either<DateTimeOffset>(u.Consent.Expires.Value);
                    else
                        return new Either<DateTimeOffset>(commit.Error);
                }
                else
                    return new Either<DateTimeOffset>(c.Error);
            }
            else
                return new Either<DateTimeOffset>(user.Error);
        }
    }
}