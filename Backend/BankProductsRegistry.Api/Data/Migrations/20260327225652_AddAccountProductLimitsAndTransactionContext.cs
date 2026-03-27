using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProductsRegistry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountProductLimitsAndTransactionContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Transactions",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TransactionChannel",
                table: "Transactions",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountProductLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountProductId = table.Column<int>(type: "int", nullable: false),
                    CreditLimitTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DailyConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PerTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AtmWithdrawalLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    InternationalConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountProductLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountProductLimits_AccountProducts_AccountProductId",
                        column: x => x.AccountProductId,
                        principalTable: "AccountProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountProductLimitTemporaryAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountProductId = table.Column<int>(type: "int", nullable: false),
                    CreditLimitTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DailyConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PerTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AtmWithdrawalLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    InternationalConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedByUserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountProductLimitTemporaryAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountProductLimitTemporaryAdjustments_AccountProducts_Acco~",
                        column: x => x.AccountProductId,
                        principalTable: "AccountProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountProductLimitHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountProductId = table.Column<int>(type: "int", nullable: false),
                    TemporaryAdjustmentId = table.Column<int>(type: "int", nullable: true),
                    ChangeType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousCreditLimitTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewCreditLimitTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousDailyConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewDailyConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousPerTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewPerTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousAtmWithdrawalLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewAtmWithdrawalLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousInternationalConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NewInternationalConsumptionLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Reason = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountProductLimitHistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountProductLimitHistoryEntries_AccountProductLimitTempora~",
                        column: x => x.TemporaryAdjustmentId,
                        principalTable: "AccountProductLimitTemporaryAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountProductLimitHistoryEntries_AccountProducts_AccountPro~",
                        column: x => x.AccountProductId,
                        principalTable: "AccountProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductLimitHistoryEntries_AccountProductId_CreatedAt",
                table: "AccountProductLimitHistoryEntries",
                columns: new[] { "AccountProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductLimitHistoryEntries_TemporaryAdjustmentId",
                table: "AccountProductLimitHistoryEntries",
                column: "TemporaryAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductLimits_AccountProductId",
                table: "AccountProductLimits",
                column: "AccountProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductLimitTemporaryAdjustments_AccountProductId_Sta~",
                table: "AccountProductLimitTemporaryAdjustments",
                columns: new[] { "AccountProductId", "StartsAt", "EndsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountProductLimitHistoryEntries");

            migrationBuilder.DropTable(
                name: "AccountProductLimits");

            migrationBuilder.DropTable(
                name: "AccountProductLimitTemporaryAdjustments");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionChannel",
                table: "Transactions");
        }
    }
}
