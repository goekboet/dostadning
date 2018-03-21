using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using dostadning.domain.ourdata;
using dostadning.domain.result;
using Microsoft.EntityFrameworkCore;

namespace dostadning.records.pgres
{
    
    public class Pgres : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        private string CS { get; } = Environment.GetEnvironmentVariable("dostadning_records_pgres_cs");
        protected override void OnConfiguring(DbContextOptionsBuilder opt)
        {
            opt.UseNpgsql(CS);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountsTable());
            modelBuilder.ApplyConfiguration(new TraderaUsersTable());
        }
    }
    public static class Repos
    {
        public static IRepository<Account, Guid> Accounts(Pgres db) => new Accounts(db);
    }
    
    internal class Repository<T> 
    {
        public Repository(Pgres db) => Db = db;
        protected Pgres Db { get; }
        public void Dispose() => Db.Dispose();
    }
}
