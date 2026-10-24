using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class HardenTenantAuthAndBranchLifecycle : Migration
    {
        private static readonly string[] BranchReferenceTables =
        [
            "AspNetUsers", "BranchPhones", "Warehouses", "PurchaseOrders", "PurchaseInvoices", "PurchaseReturns",
            "SalesInvoices", "SalesReturns", "CustomerTransactions", "SupplierTransactions", "Safes", "Expenses",
            "PosTerminals", "CashSessions"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Do not rename business data or reassign login identifiers automatically.
            // These checks run before any schema changes and the migration is transactional.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Branches] WHERE [IsDeleted] = 0
                           GROUP BY [TenantId], [Name] HAVING COUNT(*) > 1)
                    THROW 51000, 'HardenTenantAuthAndBranchLifecycle: resolve duplicate active branch names before migrating.', 1;
                IF EXISTS (
                    SELECT a.[Value] FROM [AspNetUsers] u
                    CROSS APPLY (VALUES (u.[NormalizedUserName]), (u.[NormalizedEmail])) a([Value])
                    WHERE a.[Value] IS NOT NULL AND a.[Value] <> N''
                    GROUP BY a.[Value] HAVING COUNT(DISTINCT u.[Id]) > 1)
                    THROW 51000, 'HardenTenantAuthAndBranchLifecycle: resolve username/email login collisions before migrating.', 1;
                """);

            migrationBuilder.CreateTable(
                name: "UserLoginIdentifiers",
                columns: table => new
                {
                    NormalizedValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginIdentifiers", x => x.NormalizedValue);
                    table.ForeignKey(
                        name: "FK_UserLoginIdentifiers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_Name",
                table: "Branches",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginIdentifiers_UserId",
                table: "UserLoginIdentifiers",
                column: "UserId");

            migrationBuilder.Sql("""
                INSERT INTO [UserLoginIdentifiers] ([NormalizedValue], [UserId])
                SELECT DISTINCT a.[Value], u.[Id] FROM [AspNetUsers] u
                CROSS APPLY (VALUES (u.[NormalizedUserName]), (u.[NormalizedEmail])) a([Value])
                WHERE a.[Value] IS NOT NULL AND a.[Value] <> N'';

                UPDATE p SET [IsDeleted] = 1, [UpdatedAt] = SYSUTCDATETIME()
                FROM [BranchPhones] p INNER JOIN [Branches] b ON p.[TenantId] = b.[TenantId] AND p.[BranchId] = b.[Id]
                WHERE b.[IsDeleted] = 1 AND p.[IsDeleted] = 0;
                """);

            // One unique namespace protects usernames AND emails, including concurrent writers
            // and writers outside Identity. The trigger participates in the user transaction.
            CreateTrigger(migrationBuilder, """
                CREATE TRIGGER [TR_AspNetUsers_LoginIdentifiers] ON [AspNetUsers]
                AFTER INSERT, UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @Changed TABLE ([UserId] nvarchar(450) PRIMARY KEY);
                    INSERT INTO @Changed
                    SELECT i.[Id] FROM inserted i LEFT JOIN deleted d ON i.[Id] = d.[Id]
                    WHERE d.[Id] IS NULL OR EXISTS (
                        SELECT i.[NormalizedUserName], i.[NormalizedEmail]
                        EXCEPT SELECT d.[NormalizedUserName], d.[NormalizedEmail]);

                    DELETE li FROM [UserLoginIdentifiers] li INNER JOIN @Changed c ON c.[UserId] = li.[UserId]
                    WHERE NOT EXISTS (
                        SELECT 1 FROM inserted i
                        CROSS APPLY (VALUES (i.[NormalizedUserName]), (i.[NormalizedEmail])) a([Value])
                        WHERE i.[Id] = c.[UserId] AND a.[Value] = li.[NormalizedValue]);

                    INSERT INTO [UserLoginIdentifiers] ([NormalizedValue], [UserId])
                    SELECT DISTINCT a.[Value], i.[Id] FROM inserted i INNER JOIN @Changed c ON c.[UserId] = i.[Id]
                    CROSS APPLY (VALUES (i.[NormalizedUserName]), (i.[NormalizedEmail])) a([Value])
                    WHERE a.[Value] IS NOT NULL AND a.[Value] <> N'' AND NOT EXISTS (
                        SELECT 1 FROM [UserLoginIdentifiers] li
                        WHERE li.[NormalizedValue] = a.[Value] AND li.[UserId] = i.[Id]);
                END
                """);

            foreach (var table in BranchReferenceTables)
                CreateTrigger(migrationBuilder, $$"""
                    CREATE TRIGGER [TR_{{table}}_LiveBranch] ON [{{table}}]
                    AFTER INSERT, UPDATE AS
                    BEGIN
                        SET NOCOUNT ON;
                        IF EXISTS (
                            SELECT 1 FROM inserted i
                            LEFT JOIN deleted d ON i.[Id] = d.[Id]
                            INNER JOIN [Branches] b WITH (HOLDLOCK)
                                ON b.[TenantId] = i.[TenantId] AND b.[Id] = i.[BranchId]
                            WHERE b.[IsDeleted] = 1 AND (d.[Id] IS NULL OR EXISTS (
                                SELECT i.[TenantId], i.[BranchId] EXCEPT SELECT d.[TenantId], d.[BranchId])))
                            THROW 51002, 'Cannot attach data to a deleted branch.', 1;
                    END
                    """);

            var dependencies = string.Join(" OR ", BranchReferenceTables.Where(t => t != "BranchPhones")
                .Select(t => $"EXISTS (SELECT 1 FROM [{t}] r WHERE r.[TenantId] = i.[TenantId] AND r.[BranchId] = i.[Id])"));
            CreateTrigger(migrationBuilder, $$"""
                CREATE TRIGGER [TR_Branches_DeleteGuard] ON [Branches]
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    IF UPDATE([IsDeleted]) AND EXISTS (
                        SELECT 1 FROM inserted i INNER JOIN deleted d ON i.[Id] = d.[Id]
                        WHERE i.[IsDeleted] = 1 AND d.[IsDeleted] = 0 AND ({{dependencies}}))
                        THROW 51001, 'Cannot delete a branch with users, warehouses or operational history.', 1;
                END
                """);
        }

        // CREATE TRIGGER must be first in its batch, including inside EF's idempotent IF wrappers.
        private static void CreateTrigger(MigrationBuilder migrationBuilder, string sql) =>
            migrationBuilder.Sql($"EXEC(N'{sql.Replace("'", "''")}');");

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [TR_Branches_DeleteGuard]; DROP TRIGGER IF EXISTS [TR_AspNetUsers_LoginIdentifiers];");
            foreach (var table in BranchReferenceTables)
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS [TR_{table}_LiveBranch];");

            migrationBuilder.DropTable(
                name: "UserLoginIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_Branches_TenantId_Name",
                table: "Branches");
        }
    }
}
