using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using dostadning.domain.ourdata;
using dostadning.domain.result;
using dostadning.domain.service.tradera;
using Dto = dostadning.domain.features;
using Our = dostadning.domain.ourdata;

namespace dostadning.domain.features
{
    public static class GetTraderaConsentFeature
    {
        public static IObservable<int> PairWithId(
            IAuthorizationCalls soap,
            string alias) => soap.Identify(alias);

        static Account Associate(
            this Account a,
            TraderaUser u)
        {
            a.TraderaUsers.Add(u);

            return a;
        }

        public static TraderaConsent Consent(
            this Account u,
            string alias) =>
            u.TraderaUser(alias).Consent;

        static TraderaConsent Init(Guid id) => new TraderaConsent { Id = id };
        public static IObservable<string> AddTraderaUser(
            IRepository<Account, Guid> users,
            IAuthorizationCalls soap,
            string claim,
            string alias) =>
                from id in soap.Identify(alias)
                from oldstate in users.Find(Guid.Parse(claim))
                let newstate = oldstate.Associate(new TraderaUser
                {
                    Id = id,
                    Alias = alias,
                    Consent = Init(Guid.NewGuid())
                })
                from _ in users.Commit()
                select newstate.Consent(alias).Id.ToString()
                ;

        public static IObservable<DateTimeOffset> FetchConsent(
            IRepository<Account, Guid> users,
            IAuthorizationCalls soap,
            string claim,
            string alias) =>
                from u in users.Find(Guid.Parse(claim)).Select(x => x.TraderaUser(alias))
                from t in soap.FetchToken(u.Id, u.Consent.Id.ToString())
                let exp = u.UpdateConsent(t)
                from _ in users.Commit()
                select exp;
        static DateTimeOffset UpdateConsent(this TraderaUser u, Token t)
        {
            u.Consent.Token = t.Id;
            u.Consent.Expires = t.Expires;

            return u.Consent.Expires.Value;
        }

        public static TraderaUser TraderaUser(
            this Account u,
            string alias)
        {
            try
            { return u.TraderaUsers.Single(x => x.Alias.Equals(alias)); }
            catch (InvalidOperationException e)
            { throw new Error($"Traderaalias {alias} not found on account {u.Id}", e); }
        }
    }
}