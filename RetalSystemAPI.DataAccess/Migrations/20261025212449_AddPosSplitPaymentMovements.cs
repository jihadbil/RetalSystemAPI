using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPosSplitPaymentMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashSessionPaymentSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashSessionPaymentSettlements", x => x.Id);
                    table.UniqueConstraint("AK_CashSessionPaymentSettlements_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_CashSessionPaymentSettlements_CashSessions_TenantId_CashSessionId",
                        columns: x => new { x.TenantId, x.CashSessionId },
                        principalTable: "CashSessions",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessionPaymentSettlements_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashSessionMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientMovementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TenderedAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReversedAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OriginalMovementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SettlementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SettlementSafeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashSessionMovements", x => x.Id);
                    table.UniqueConstraint("AK_CashSessionMovements_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.CheckConstraint("CK_CashSessionMovements_Amounts", "[Amount] > 0 AND [ReversedAmount] >= 0 AND [ReversedAmount] <= [Amount]");
                    table.CheckConstraint("CK_CashSessionMovements_Direction", "[Direction] IN (1,2)");
                    table.CheckConstraint("CK_CashSessionMovements_Method", "[PaymentMethod] IN (1,2,3,5)");
                    table.CheckConstraint("CK_CashSessionMovements_Tendered", "[TenderedAmount] IS NULL OR ([PaymentMethod] = 1 AND [TenderedAmount] >= [Amount])");
                    table.ForeignKey(
                        name: "FK_CashSessionMovements_CashSessionMovements_TenantId_OriginalMovementId",
                        columns: x => new { x.TenantId, x.OriginalMovementId },
                        principalTable: "CashSessionMovements",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessionMovements_CashSessionPaymentSettlements_TenantId_SettlementId",
                        columns: x => new { x.TenantId, x.SettlementId },
                        principalTable: "CashSessionPaymentSettlements",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessionMovements_CashSessions_TenantId_CashSessionId",
                        columns: x => new { x.TenantId, x.CashSessionId },
                        principalTable: "CashSessions",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessionMovements_Safes_TenantId_SettlementSafeId",
                        columns: x => new { x.TenantId, x.SettlementSafeId },
                        principalTable: "Safes",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessionMovements_SalesInvoices_TenantId_SalesInvoiceId",
                        columns: x => new { x.TenantId, x.SalesInvoiceId },
                        principalTable: "SalesInvoices",
                        principalColumns: new[] { "TenantId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessionMovements_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionMovements_TenantId_CashSessionId_SettlementId",
                table: "CashSessionMovements",
                columns: new[] { "TenantId", "CashSessionId", "SettlementId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionMovements_TenantId_DocumentType_DocumentId_ClientMovementId",
                table: "CashSessionMovements",
                columns: new[] { "TenantId", "DocumentType", "DocumentId", "ClientMovementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionMovements_TenantId_OriginalMovementId",
                table: "CashSessionMovements",
                columns: new[] { "TenantId", "OriginalMovementId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionMovements_TenantId_SalesInvoiceId",
                table: "CashSessionMovements",
                columns: new[] { "TenantId", "SalesInvoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionMovements_TenantId_SettlementId",
                table: "CashSessionMovements",
                columns: new[] { "TenantId", "SettlementId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionMovements_TenantId_SettlementSafeId",
                table: "CashSessionMovements",
                columns: new[] { "TenantId", "SettlementSafeId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessionPaymentSettlements_TenantId_CashSessionId",
                table: "CashSessionPaymentSettlements",
                columns: new[] { "TenantId", "CashSessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashSessionMovements");

            migrationBuilder.DropTable(
                name: "CashSessionPaymentSettlements");
        }
    }
}
