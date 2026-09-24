using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddHybridPosCashSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CashSessionId",
                table: "SalesInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PosTerminalId",
                table: "SalesInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CashSessionId",
                table: "SafeTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "SafeTransactions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserId",
                table: "SafeTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedAt",
                table: "SafeTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PosTerminals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultCashDrawerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PairingCodeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PairingCodeExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PosTerminals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosTerminals_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosTerminals_Safes_DefaultCashDrawerId",
                        column: x => x.DefaultCashDrawerId,
                        principalTable: "Safes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PosTerminals_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosTerminalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashDrawerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpeningFloat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ExpectedClosingBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CountedClosingBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Difference = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClosingNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_CashSessions", x => x.Id);
                    table.CheckConstraint("CK_CashSessions_Amounts", "[OpeningFloat] >= 0 AND [ExpectedClosingBalance] >= 0 AND ([CountedClosingBalance] IS NULL OR [CountedClosingBalance] >= 0)");
                    table.CheckConstraint("CK_CashSessions_Status", "[Status] IN (1,2,3,4)");
                    table.ForeignKey(
                        name: "FK_CashSessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessions_PosTerminals_PosTerminalId",
                        column: x => x.PosTerminalId,
                        principalTable: "PosTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessions_Safes_CashDrawerId",
                        column: x => x.CashDrawerId,
                        principalTable: "Safes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSessions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PosDeviceCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PosTerminalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PosDeviceCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosDeviceCredentials_PosTerminals_PosTerminalId",
                        column: x => x.PosTerminalId,
                        principalTable: "PosTerminals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosDeviceCredentials_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Normalize values written by the legacy desktop/web clients before
            // their enum contracts were aligned with the API.
            migrationBuilder.Sql("""
                UPDATE [Safes]
                SET [Type] = 3
                WHERE [Type] = 2
                  AND ([Name] LIKE N'%درج%' OR [Name] LIKE N'%كاشير%' OR [Name] LIKE N'%POS%');

                UPDATE [Safes] SET [Type] = 2 WHERE [Type] = 0;
                UPDATE [Expenses] SET [PaymentMethod] = 1 WHERE [PaymentMethod] NOT IN (1,2,3,5);
                UPDATE [CustomerTransactions] SET [PaymentMethod] = 1 WHERE [PaymentMethod] = 0;
                UPDATE [SupplierTransactions] SET [PaymentMethod] = 1 WHERE [PaymentMethod] = 0;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SupplierTransactions_DebitCredit",
                table: "SupplierTransactions",
                sql: "([DebitAmount] > 0 AND [CreditAmount] = 0) OR ([CreditAmount] > 0 AND [DebitAmount] = 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SupplierTransactions_DocumentType",
                table: "SupplierTransactions",
                sql: "[DocumentType] BETWEEN 1 AND 13");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SupplierTransactions_PaymentMethod",
                table: "SupplierTransactions",
                sql: "[PaymentMethod] IS NULL OR [PaymentMethod] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SupplierTransactions_Type",
                table: "SupplierTransactions",
                sql: "[TransactionType] BETWEEN 1 AND 6");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CashSessionId",
                table: "SalesInvoices",
                column: "CashSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_PosTerminalId",
                table: "SalesInvoices",
                column: "PosTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_TenantId_CashSessionId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "CashSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransactions_CashSessionId",
                table: "SafeTransactions",
                column: "CashSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransactions_TenantId_CashSessionId_TransactionDate",
                table: "SafeTransactions",
                columns: new[] { "TenantId", "CashSessionId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransactions_TenantId_IdempotencyKey",
                table: "SafeTransactions",
                columns: new[] { "TenantId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SafeTransactions_Amount",
                table: "SafeTransactions",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SafeTransactions_Direction",
                table: "SafeTransactions",
                sql: "[Direction] IN (1,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SafeTransactions_DocumentType",
                table: "SafeTransactions",
                sql: "[DocumentType] BETWEEN 1 AND 13");

            migrationBuilder.CreateIndex(
                name: "IX_Safes_TenantId_BranchId_Name",
                table: "Safes",
                columns: new[] { "TenantId", "BranchId", "Name" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Safes_Balances",
                table: "Safes",
                sql: "[OpeningBalance] >= 0 AND [CurrentBalance] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Safes_Type",
                table: "Safes",
                sql: "[Type] IN (1,2,3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Expenses_Amount",
                table: "Expenses",
                sql: "[Amount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Expenses_PaymentMethod",
                table: "Expenses",
                sql: "[PaymentMethod] IN (1,2,3,5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerTransactions_DebitCredit",
                table: "CustomerTransactions",
                sql: "([DebitAmount] > 0 AND [CreditAmount] = 0) OR ([CreditAmount] > 0 AND [DebitAmount] = 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerTransactions_DocumentType",
                table: "CustomerTransactions",
                sql: "[DocumentType] BETWEEN 1 AND 13");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerTransactions_PaymentMethod",
                table: "CustomerTransactions",
                sql: "[PaymentMethod] IS NULL OR [PaymentMethod] BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CustomerTransactions_Type",
                table: "CustomerTransactions",
                sql: "[TransactionType] BETWEEN 1 AND 6");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_BranchId",
                table: "CashSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_CashDrawerId",
                table: "CashSessions",
                column: "CashDrawerId",
                unique: true,
                filter: "[Status] IN (1,2) AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_CashierUserId",
                table: "CashSessions",
                column: "CashierUserId",
                unique: true,
                filter: "[Status] IN (1,2) AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_PosTerminalId",
                table: "CashSessions",
                column: "PosTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_TenantId_OpenedAt",
                table: "CashSessions",
                columns: new[] { "TenantId", "OpenedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PosDeviceCredentials_PosTerminalId",
                table: "PosDeviceCredentials",
                column: "PosTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_PosDeviceCredentials_TenantId_PosTerminalId_RevokedAt",
                table: "PosDeviceCredentials",
                columns: new[] { "TenantId", "PosTerminalId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PosDeviceCredentials_TokenHash",
                table: "PosDeviceCredentials",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_BranchId",
                table: "PosTerminals",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_DefaultCashDrawerId",
                table: "PosTerminals",
                column: "DefaultCashDrawerId");

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_TenantId_BranchId_Name",
                table: "PosTerminals",
                columns: new[] { "TenantId", "BranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_TenantId_Code",
                table: "PosTerminals",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_TenantId_IsDeleted",
                table: "PosTerminals",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.AddForeignKey(
                name: "FK_SafeTransactions_CashSessions_CashSessionId",
                table: "SafeTransactions",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_CashSessions_CashSessionId",
                table: "SalesInvoices",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_PosTerminals_PosTerminalId",
                table: "SalesInvoices",
                column: "PosTerminalId",
                principalTable: "PosTerminals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafeTransactions_CashSessions_CashSessionId",
                table: "SafeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_CashSessions_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_PosTerminals_PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropTable(
                name: "CashSessions");

            migrationBuilder.DropTable(
                name: "PosDeviceCredentials");

            migrationBuilder.DropTable(
                name: "PosTerminals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SupplierTransactions_DebitCredit",
                table: "SupplierTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SupplierTransactions_DocumentType",
                table: "SupplierTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SupplierTransactions_PaymentMethod",
                table: "SupplierTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SupplierTransactions_Type",
                table: "SupplierTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_TenantId_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SafeTransactions_CashSessionId",
                table: "SafeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SafeTransactions_TenantId_CashSessionId_TransactionDate",
                table: "SafeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SafeTransactions_TenantId_IdempotencyKey",
                table: "SafeTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SafeTransactions_Amount",
                table: "SafeTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SafeTransactions_Direction",
                table: "SafeTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SafeTransactions_DocumentType",
                table: "SafeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Safes_TenantId_BranchId_Name",
                table: "Safes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Safes_Balances",
                table: "Safes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Safes_Type",
                table: "Safes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Expenses_Amount",
                table: "Expenses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Expenses_PaymentMethod",
                table: "Expenses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerTransactions_DebitCredit",
                table: "CustomerTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerTransactions_DocumentType",
                table: "CustomerTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerTransactions_PaymentMethod",
                table: "CustomerTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CustomerTransactions_Type",
                table: "CustomerTransactions");

            migrationBuilder.DropColumn(
                name: "CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CashSessionId",
                table: "SafeTransactions");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "SafeTransactions");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                table: "SafeTransactions");

            migrationBuilder.DropColumn(
                name: "PostedAt",
                table: "SafeTransactions");
        }
    }
}
