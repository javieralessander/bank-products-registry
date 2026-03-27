using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProductsRegistry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountProductBlocksAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountProductBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountProductId = table.Column<int>(type: "int", nullable: false),
                    BlockType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    AppliedByUserId = table.Column<int>(type: "int", nullable: true),
                    AppliedByUserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    ReleasedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReleasedByUserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReleaseReason = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountProductBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountProductBlocks_AccountProducts_AccountProductId",
                        column: x => x.AccountProductId,
                        principalTable: "AccountProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountProductAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountProductId = table.Column<int>(type: "int", nullable: false),
                    AccountProductBlockId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Detail = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountProductAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountProductAuditEntries_AccountProductBlocks_AccountProdu~",
                        column: x => x.AccountProductBlockId,
                        principalTable: "AccountProductBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountProductAuditEntries_AccountProducts_AccountProductId",
                        column: x => x.AccountProductId,
                        principalTable: "AccountProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductAuditEntries_AccountProductBlockId",
                table: "AccountProductAuditEntries",
                column: "AccountProductBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductAuditEntries_AccountProductId_CreatedAt",
                table: "AccountProductAuditEntries",
                columns: new[] { "AccountProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountProductBlocks_AccountProductId_ReleasedAt_StartsAt",
                table: "AccountProductBlocks",
                columns: new[] { "AccountProductId", "ReleasedAt", "StartsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountProductAuditEntries");

            migrationBuilder.DropTable(
                name: "AccountProductBlocks");
        }
    }
}
