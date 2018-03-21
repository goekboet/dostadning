using System;
using System.Linq;
using System.Reactive.Linq;
using dostadning.domain.features;
using dostadning.domain.ourdata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dostadning.records.pgres
{
    internal sealed class Accounts :
        Repository<Account>,
        IRepository<Account, Guid>
    {
        public Accounts(Pgres ctx) : base(ctx) { }

        public IQueryable<Account> Store => Db.Accounts.AsNoTracking();

        public IRepository<Account, Guid> Add(Account t) { Db.Add(t); return this; }

        public IObservable<Account> Find(Guid key) => 
            Observable.FromAsync(() => Db.Accounts
            .Include(x => x.TraderaUsers)
                .ThenInclude(x => x.Consent)
            .SingleOrDefaultAsync(x => x.Id == key));

        public IObservable<int> Commit() => Observable
            .FromAsync(() => Db.SaveChangesAsync())
            .Catch<int, DbUpdateException>(e => 
                Observable.Throw<int>(new Error($"Pgres failed to update Account.", e))
            );
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