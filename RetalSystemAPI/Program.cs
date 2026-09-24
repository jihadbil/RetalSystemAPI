using System.Text;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Extensions;
using RetalSystemAPI.HealthChecks;
using RetalSystemAPI.Models;
using RetalSystemAPI.Services.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "frontend-clients";

// Add DataAccess layer services
builder.Services.AddDataAccess(builder.Configuration);

// Add Services layer services
builder.Services.AddServicesLayer(builder.Configuration);

// Prevent claim mapping distortion
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// JWT configuration must be supplied by a protected configuration provider
// (environment variable, user-secrets, Key Vault, etc.). Never fall back to a
// source-controlled signing key.
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is required and must contain at least 32 bytes. " +
        "Configure it through a protected configuration provider.");
}

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer is required.");
var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JwtSettings:Audience is required.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenSecurityStamp = context.Principal?.FindFirstValue("SecurityStamp");
            if (string.IsNullOrWhiteSpace(userId) || tokenSecurityStamp is null)
            {
                context.Fail("The access token is missing required session claims.");
                return;
            }

            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId);
            if (user is null ||
                !string.Equals(user.SecurityStamp ?? string.Empty, tokenSecurityStamp, StringComparison.Ordinal) ||
                (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow))
            {
                context.Fail("The access token session is no longer active.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var tenantIsActive = await dbContext.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(tenant => tenant.Id == user.TenantId && tenant.IsActive && !tenant.IsDeleted);
            var branchIsActive = await dbContext.Branches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(branch =>
                    branch.Id == user.BranchId &&
                    branch.TenantId == user.TenantId &&
                    branch.IsActive &&
                    !branch.IsDeleted);

            if (!tenantIsActive || !branchIsActive)
            {
                context.Fail("The tenant or branch assigned to this session is inactive.");
            }
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            configuredCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
            (builder.Environment.IsDevelopment() &&
             Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
             uri.IsLoopback &&
             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(
        "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"],
        timeout: TimeSpan.FromSeconds(5));
var maxExcelUploadSizeMb = Math.Clamp(builder.Configuration.GetValue<int?>("ExcelImport:MaxFileSizeMB") ?? 10, 1, 100);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxExcelUploadSizeMb * 1024L * 1024L;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-client",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddConcurrencyLimiter("bulk-operations", limiter =>
    {
        limiter.PermitLimit = 2;
        limiter.QueueLimit = 2;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

// Database migrations are an explicit deployment action in production. Local
// development can opt in through DatabaseInitialization:ApplyMigrationsOnStartup.
if (builder.Configuration.GetValue<bool>("DatabaseInitialization:ApplyMigrationsOnStartup"))
{
    await RetalSystemAPI.DataAccess.Seeders.DbInitializer.SeedAsync(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

var webRootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});

app.Run();
