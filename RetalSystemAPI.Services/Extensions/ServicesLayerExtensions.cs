using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Models.DTOs.MappingProfiles;
using RetalSystemAPI.Services.Auth.Implementations;
using RetalSystemAPI.Services.Auth.Interfaces;
using RetalSystemAPI.Services.Branch.Implementations;
using RetalSystemAPI.Services.Branch.Interfaces;
using RetalSystemAPI.Services.Catalog.Implementations;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Customers.Implementations;
using RetalSystemAPI.Services.Customers.Interfaces;
using RetalSystemAPI.Services.FileUpload.Implementations;
using RetalSystemAPI.Services.FileUpload.Interfaces;
using RetalSystemAPI.Services.Purchase.Implementations;
using RetalSystemAPI.Services.Purchase.Interfaces;
using RetalSystemAPI.Services.Sales.Implementations;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Services.Suppliers.Implementations;
using RetalSystemAPI.Services.Suppliers.Interfaces;
using RetalSystemAPI.Services.Tenant.Implementations;
using RetalSystemAPI.Services.Tenant.Interfaces;
using RetalSystemAPI.Services.Warehouses.Implementations;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Services.Extensions;

/// <summary>
/// امتدادات تسجيل خدمات طبقة الخدمات (Services Layer) في حاوية الاعتمادية (Dependency Injection Container).
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
            typeof(TenantMappingProfile).Assembly,
            typeof(SupplierMappingProfile).Assembly,
            typeof(WarehouseMappingProfile).Assembly,
            typeof(PurchaseMappingProfile).Assembly
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

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<ISalesReturnService, SalesReturnService>();

        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockTransferService, StockTransferService>();
        services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<RetalSystemAPI.Services.Users.Interfaces.IUserService, RetalSystemAPI.Services.Users.Implementations.UserService>();

        return services;
    }
}
