using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using dostadning.domain.account;

namespace dostadning.domain.seller
{
    public static class SellerFuntions
    {
        public static ConsentStatus ComputeStatus(bool hasConsent, DateTimeOffset now, DateTimeOffset exp)
        {
            if (hasConsent)
            {
                return now > exp 
                    ? ConsentStatus.Expired
                    : ConsentStatus.Given;
            }
            else
            {
                return ConsentStatus.NotGiven;
            }
        }

        public static TraderaAlias ToDomain(
            Guid acct,
            int id,
            string alias,
            bool hasConsent,
            Guid consentId,
            DateTimeOffset now,
            DateTimeOffset expires) 
            => new TraderaAlias(new Seller(acct, id), alias, consentId, ComputeStatus(hasConsent, now, expires));
        public static IObservable<IEnumerable<TraderaAlias>> List(
            IQuery<Account> users,
            AppIdentity app,
            DateTimeOffset now,
            Guid account) => users
                .Query(us => us
                    .Where(x => x.Id == account)
                    .SelectMany(x => x.TraderaUsers)
                    .Select(x => new 
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        ConsentId = x.Consent.Id,
                        HasConsent = x.Consent.Token != null,
                        Expires = x.Consent.Expires
                    }))
                .Select(x => x
                    .Select(q => ToDomain(
                        account,
                        q.Id, 
                        q.Alias, 
                        q.HasConsent,
                        q.ConsentId, 
                        now, 
                        q.Expires ?? default(DateTimeOffset))));
                

        public static IObservable<Consent> Get(
            IQuery<Account> users,
            Seller s) => 
            users.Query(us => us
                .Where(x => x.Id == s.Account)
                .SelectMany(x => x.TraderaUsers
                    .Where(tu => tu.Id == s.TraderaUser && tu.Consent.Token != null))
                .Select(x => new
                {
                    Id = x.Id,
                    Token = x.Consent.Token
                }))
            .Select(x => x.SingleOrDefault())
            .Where(x => x != null)
            .Select(x => new Consent(s, x.Token));

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
            int id) =>
            u.TraderaUser(id).Consent;

        static TraderaConsent Init(Guid id) => new TraderaConsent { Id = id };
        public static IObservable<TraderaAlias> AddTraderaUser(
            IDataCommand<Account, Guid> users,
            IAuthorizationCalls soap,
            AppIdentity app,
            Guid account,
            string alias) =>
                from id in soap.Identify(alias)
                from oldstate in users.Find(account)
                let newstate = oldstate.Associate(new TraderaUser
                {
                    Id = id,
                    Alias = alias,
                    Consent = Init(Guid.NewGuid())
                })
                from _ in users.Commit()
                select new TraderaAlias(
                    new Seller(account, id), 
                    alias, 
                    newstate.Consent(id).Id,
                    ConsentStatus.NotGiven)
                ;

        public static IObservable<Consent> FetchConsent(
            IDataCommand<TraderaUser, Seller> users,
            IAuthorizationCalls soap,
            Seller s) =>
                from u in users.Find(s)
                from t in soap.FetchToken(u.Id, u.Consent.Id.ToString())
                let exp = u.RecordConsent(t, s)
                from _ in users.Commit()
                select exp;
        static Consent RecordConsent(this TraderaUser u, Token t, Seller s)
        {
            u.Consent.Token = t.Id;
            u.Consent.Expires = t.Expires;

            return new Consent(s, t.Id);
        }

        public static TraderaUser TraderaUser(
            this Account u,
            int id)
        {
            try
            { return u.TraderaUsers.Single(x => x.Id == id); }
            catch (InvalidOperationException e)
            { throw new Error($"TraderaId {id} not found on account {u.Id}", e); }
        }
    }
}