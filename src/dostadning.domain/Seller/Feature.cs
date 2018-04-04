using System;
using System.Collections.Generic;
using dostadning.domain.account;

using S = dostadning.domain.seller.SellerFuntions;

namespace dostadning.domain.seller
{
    public interface ISeller
    {
        IObservable<TraderaAlias> Add(Guid account, string traderaAlias);
        IObservable<Consent> Confirm(Seller s, Guid consentId);
        IObservable<IEnumerable<TraderaAlias>> List(Guid account);
        IObservable<Consent> Get(Seller s);
    }

    public class SellerFeature : ISeller
    {
        IRepository<Account, Guid> Repo { get; }
        IAuthorizationCalls Soap { get; }
        AppIdentity App { get; }
        Func<DateTimeOffset> Now { get; }

        public SellerFeature(
            IRepository<Account, Guid> repo,
            IAuthorizationCalls soap,
            AppIdentity app,
            Func<DateTimeOffset> now)
        {
            Repo = repo;
            Soap = soap;
            App = app;
            Now = now;
        }

        public IObservable<TraderaAlias> Add(Guid account, string traderaAlias) =>
            S.AddTraderaUser(Repo, Soap, App, account, traderaAlias);

        public IObservable<Consent> Confirm(Seller s, Guid consentId) =>
            S.FetchConsent(Repo, Soap, s, consentId);

        public IObservable<IEnumerable<TraderaAlias>> List(Guid account) =>
            S.List(Repo, App, Now(), account);

        public IObservable<Consent> Get(Seller s) =>
            S.Get(Repo, s);
    }

    public sealed class Consent
    {
        public Consent(
            Seller id,
            string token)
        { Id = id; Token = token; }

        public Seller Id { get; }
        public string Token { get; }

        public string n => Environment.NewLine;
        public override string ToString() => 
            $"Seller: {Id}{n}" +
            $"Consent: {Token}";
    }

    /// <summary>
    /// An auction is always held by a user of dostadning that has given
    /// consent to a TraderaUser.
    /// </summary>
    public struct Seller
    {
        public Seller(Guid acct, int tUser)
        {
            Account = acct;
            TraderaUser = tUser;
        }

        public Guid Account { get; }
        public int TraderaUser { get; }

        public override string ToString() => 
            $"{Account}/{TraderaUser}";
    }

    public enum ConsentStatus { Given, NotGiven, Expired };

    public sealed class TraderaAlias
    {
        public TraderaAlias(
            Seller s,
            string alias,
            Guid consentId,
            ConsentStatus status)
        {
            Seller = s;
            Alias = alias;
            ConsentId = consentId;
            ConsentStatus = status;
        }

        public string Alias { get; }
        public Seller Seller { get; }
        public Guid ConsentId { get; }
        public string ConsentUrl(AppIdentity app) =>
             $"https://api.tradera.com/tokenlogin.aspx?appId={app.Id}&pkey={app.PKey}&skey={ConsentId.ToString()}";
        public ConsentStatus ConsentStatus { get; }

        string n => Environment.NewLine;
        public override string ToString() =>
            $"Alias: {Alias}{n}" +
            $"Seller: {Seller}{n}" +
            $"ConsentId: {ConsentId}{n}" +
            $"ConsentStatus: {ConsentStatus}";

    }
}