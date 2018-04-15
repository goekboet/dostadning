using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using dostadning.domain;
using dostadning.domain.account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dostadning.records.pgres
{
    internal sealed class Accounts :
        Repository<Account>,
        IRepository<Account, Guid>
    {
        public Accounts(Pgres ctx) : base(ctx) { }

        private IQueryable<Account> Store => Db.Accounts.AsNoTracking();

        public IDataCommand<Account, Guid> Add(Account t) { Db.Add(t); return this; }

        public IObservable<Account> Find(Guid key) => 
            Observable.FromAsync(() => Db.Accounts
            .Include(x => x.TraderaUsers)
                .ThenInclude(x => x.Consent)
            .SingleOrDefaultAsync(x => x.Id == key));

        public IObservable<IEnumerable<T2>> Query<T2>(Func<IQueryable<Account>, IQueryable<T2>> q) => 
            Observable.FromAsync(() => q(Store).ToArrayAsync());
    }

    public class AccountsTable : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasMany(x => x.TraderaUsers)
                .WithOne()
                .HasForeignKey("AccountId");
        }
    }
}