﻿// <auto-generated />
using dostadning.records.pgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace dostadning.records.Migrations
{
    [DbContext(typeof(Pgres))]
    partial class PgresModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("dostadning.domain.ourdata.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("dostadning.domain.ourdata.TraderaUser", b =>
                {
                    b.Property<int>("Id");

                    b.Property<Guid>("AccountId");

                    b.Property<string>("Alias")
                        .IsRequired();

                    b.HasKey("Id", "AccountId");

                    b.HasIndex("AccountId");

                    b.ToTable("TraderaUser");
                });

            modelBuilder.Entity("dostadning.domain.ourdata.TraderaUser", b =>
                {
                    b.HasOne("dostadning.domain.ourdata.Account")
                        .WithMany("TraderaUsers")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("dostadning.domain.ourdata.TraderaConsent", "Consent", b1 =>
                        {
                            b1.Property<int>("TraderaUserId");

                            b1.Property<Guid>("TraderaUserAccountId");

                            b1.Property<DateTimeOffset?>("Expires");

                            b1.Property<Guid>("Id");

                            b1.Property<string>("Token");

                            b1.ToTable("TraderaUser");

                            b1.HasOne("dostadning.domain.ourdata.TraderaUser")
                                .WithOne("Consent")
                                .HasForeignKey("dostadning.domain.ourdata.TraderaConsent", "TraderaUserId", "TraderaUserAccountId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
