using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Models.DTOs.MappingProfiles;
using RetalSystemAPI.Services.Auth.Implementations;
using RetalSystemAPI.Services.Auth.Interfaces;
using RetalSystemAPI.Services.Branch.Implementations;
using RetalSystemAPI.Services.Branch.Interfaces;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.FileUpload.Implementations;
using RetalSystemAPI.Services.FileUpload.Interfaces;
using RetalSystemAPI.Services.Tenant.Implementations;
using RetalSystemAPI.Services.Tenant.Interfaces;

namespace RetalSystemAPI.Services.Extensions;

/// <summary>
/// امتدادات تسجيل خدمات طبقة الخدمات (Services Layer) في كاوية الاعتمادية (Dependency Injection Container).
/// </summary>
public static class ServicesLayerExtensions
{
    public static IServiceCollection AddServicesLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── AutoMapper Profiles ──────────────────────────────────
        services.AddAutoMapper(
            typeof(CatalogMappingProfile).Assembly,
            typeof(BranchMappingProfile).Assembly,
            typeof(TenantMappingProfile).Assembly
        );

        // ── Services Registration ─────────────────────────────────
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBranchService, BranchService>();

        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductExcelService, ProductExcelService>();
        services.AddScoped<IProductUnitService, ProductUnitService>();
        services.AddScoped<IProductBarCodeService, ProductBarCodeService>();
        services.AddScoped<IProductImageService, ProductImageService>();

        services.AddScoped<IFileUploadService, FileUploadService>();

        return services;
    }
}
