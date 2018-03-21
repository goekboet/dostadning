using System;
using dostadning.domain.ourdata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dostadning.records.pgres
{
    public class TraderaUsersTable : IEntityTypeConfiguration<TraderaUser>
    {
        public void Configure(EntityTypeBuilder<TraderaUser> builder)
        {
            builder.Property<Guid>("AccountId");
            builder.HasKey("Id", "AccountId");

            builder.Property(x => x.Alias).IsRequired();

            builder.OwnsOne(x => x.Consent);
        }
    }
}