using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using dostadning.domain;
using dostadning.domain.account;
using dostadning.domain.seller;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dostadning.records.pgres
{
    internal sealed class Sellers :
        Repository<TraderaUser>,
        IDataCommand<TraderaUser, Seller>
    {
        public Sellers(Pgres ctx) : base(ctx) { }

        public IDataCommand<TraderaUser, Seller> Add(TraderaUser t) { Db.Add(t); return this; }

        public IObservable<TraderaUser> Find(Seller key) =>
            Observable.FromAsync(() => Db.TraderaUser
                .Include(x => x.Consent)
                .SingleOrDefaultAsync(
                    x => x.Id == key.TraderaUser &&
                         EF.Property<Guid>(x, "AccountId") == key.Account))
                .SelectMany(x => x == null
                    ? Observable.Throw<TraderaUser>(new Error($"No seller with key {key} found"))
                    : Observable.Return(x));
    }
}