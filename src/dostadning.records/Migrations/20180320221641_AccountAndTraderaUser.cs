using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace dostadning.records.Migrations
{
    public partial class AccountAndTraderaUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TraderaUser",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    Alias = table.Column<string>(nullable: false),
                    Consent_Expires = table.Column<DateTimeOffset>(nullable: true),
                    Consent_Id = table.Column<Guid>(nullable: false),
                    Consent_Token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderaUser", x => new { x.Id, x.AccountId });
                    table.ForeignKey(
                        name: "FK_TraderaUser_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraderaUser_AccountId",
                table: "TraderaUser",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraderaUser");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
