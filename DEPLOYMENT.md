# Production configuration

The API intentionally fails fast when its signing key or database connection is
missing. Production secrets must not be committed to `appsettings*.json`.

Required deployment settings:

```text
ConnectionStrings__DefaultConnection=<production SQL Server connection string>
JwtSettings__Secret=<random secret containing at least 32 bytes>
JwtSettings__Issuer=RetalSystemAPI
JwtSettings__Audience=RetalSystemAPI-Client
AllowedHosts=<public API host name>
Cors__AllowedOrigins__0=https://app.example.com
```

The desktop application reads its endpoint from:

```text
RETAL_API_BASE_URL=https://api.example.com/api/
```

The desktop client requires a normally trusted TLS certificate in every build;
there is no certificate-validation bypass. Images are accepted only from the
same HTTPS origin configured by `RETAL_API_BASE_URL`.

Optional Excel import safety limits (shown with their defaults):

```text
ExcelImport__MaxFileSizeMB=10
ExcelImport__MaxExpandedSizeMB=50
ExcelImport__MaxArchiveEntries=1000
ExcelImport__MaxCompressionRatio=200
ExcelImport__MaxWorksheetCount=10
ExcelImport__MaxTotalRows=20000
```

Use `GET /health/live` for process liveness and `GET /health/ready` for
readiness. The readiness endpoint fails when SQL Server cannot be reached.

`FinancialReconciliation__MaxRecordsPerSource` defaults to `25000`. When a
source reaches this safety limit, the reconciliation response sets
`isTruncated=true`; rerun it per branch or raise the limit only for a controlled
maintenance job.

## Database initialization

Production migrations should be applied as a controlled deployment step. Keep
`DatabaseInitialization__ApplyMigrationsOnStartup=false` during normal startup.

For a new installation, initial platform administrator creation is opt-in. Pass
the following values through a temporary protected deployment secret, start one
instance with initialization enabled, then remove the password setting and turn
seeding off:

```text
DatabaseInitialization__ApplyMigrationsOnStartup=true
DatabaseInitialization__SeedInitialAdmin=true
DatabaseInitialization__InitialAdmin__UserName=<unique user name>
DatabaseInitialization__InitialAdmin__Email=<administrator email>
DatabaseInitialization__InitialAdmin__Password=<strong unique password>
```

The created account receives the `SuperAdmin` role. Tenant administrators use
the separate `Admin` role and cannot manage other tenants or global roles.

For local development, configure the signing key without committing it:

```powershell
dotnet user-secrets set "JwtSettings:Secret" "<local random secret of at least 32 bytes>" --project RetalSystemAPI/RetalSystemAPI.csproj
```

## Financial posting rollout

Before deploying the financial integration migrations:

1. Take and verify a restorable SQL Server backup.
2. Generate and review the idempotent migration script:

   ```powershell
   dotnet ef migrations script --idempotent --project RetalSystemAPI.DataAccess --startup-project RetalSystemAPI --context AppDbContext
   ```

3. Ensure every operating branch has at least one active safe. The desktop now
   lets the operator select the safe used by sales, purchases and returns.
4. Reconcile historical invoices separately. Documents created before
   `IntegrateFinancialDocumentPosting` have no automatic customer, supplier or
   safe posting and intentionally are not backfilled because the original safe
   cannot be inferred safely.
5. Apply migrations in a maintenance window and run a sale, cancellation,
   purchase and return smoke test before reopening the system.
6. Before applying the migrations, call `GET /api/accounting/reconciliation`
   with an authorized finance/admin account. Export and approve every critical
   issue; the endpoint is read-only and never guesses historical safes.

A commercially safe rollout requires `criticalIssues = 0`, or a signed
reconciliation record explaining every accepted historical exception. Warnings
about multiple active safes are acceptable only when every client explicitly
sends the intended safe identifier.

The integrity migration normalizes negative or inconsistent stored invoice
amounts before adding database check constraints. Review affected production
rows in the migration script and reconciliation report before deployment.
