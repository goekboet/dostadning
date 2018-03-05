using System;
using System.Linq;
using System.Threading.Tasks;
using dostadning.domain.ourdata;
using dostadning.domain.result;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dostadning.records.pgres
{

    internal sealed class Accounts :
        Repository<Account>,
        IRepository<Account, Guid>
    {
        Error AppUserNotFound { get; } = new DomainError("dostadning.records.account_not_found");
        public Accounts(Pgres ctx) : base(ctx) { }

        public IQueryable<Account> Store => Db.Accounts.AsNoTracking();

        public Account Add(Account t) { Db.Add(t); return t; }

        public async Task<Either<Account>> Find(Guid key)
        {
            var u = await Db.Accounts
            .Include(x => x.TraderaUsers)
                .ThenInclude(x => x.Consent)
            .SingleOrDefaultAsync(x => x.Id == key);

            return u != null
                ? new Either<Account>(u)
                : new Either<Account>(AppUserNotFound);
        }
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