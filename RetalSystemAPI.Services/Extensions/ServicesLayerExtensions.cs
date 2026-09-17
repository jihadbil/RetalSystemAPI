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
using RetalSystemAPI.Services.Dashboard.Implementations;
using RetalSystemAPI.Services.Dashboard.Interfaces;
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
    /// <summary>
    /// تسجيل كافة ملفات التحويل (AutoMapper Profiles) والخدمات التابعة لطبقة الخدمات في حاوية الاعتمادية.
    /// </summary>
    /// <param name="services">مجموعة الخدمات المراد التسجيل فيها</param>
    /// <param name="configuration">إعدادات وتكوينات التطبيق</param>
    /// <returns>نفس حاوية الخدمات لاستكمال بناء سلسلة التكوين</returns>
    public static IServiceCollection AddServicesLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── AutoMapper Profiles ──────────────────────────────────
        // تسجيل ملفات التحويل للكيانات والـ DTOs عبر كافة التجميعات المطلوبة
        services.AddAutoMapper(
            typeof(CatalogMappingProfile).Assembly,
            typeof(BranchMappingProfile).Assembly,
            typeof(TenantMappingProfile).Assembly,
            typeof(SupplierMappingProfile).Assembly,
            typeof(WarehouseMappingProfile).Assembly,
            typeof(PurchaseMappingProfile).Assembly
        );

        // ── Services Registration ─────────────────────────────────
        // تسجيل خدمة إدارة المستأجرين بنطاق Scoped
        services.AddScoped<ITenantService, TenantService>();

        // تسجيل خدمة المصادقة وتوليد التوكن بنطاق Scoped
        services.AddScoped<IAuthService, AuthService>();

        // تسجيل خدمة إدارة الفروع بنطاق Scoped
        services.AddScoped<IBranchService, BranchService>();

        // تسجيل خدمة وحدات القياس للمنتجات بنطاق Scoped
        services.AddScoped<IUnitService, UnitService>();

        // تسجيل خدمة تصنيفات الكتالوج بنطاق Scoped
        services.AddScoped<ICategoryService, CategoryService>();

        // تسجيل خدمة إدارة المنتجات بنطاق Scoped
        services.AddScoped<IProductService, ProductService>();

        // تسجيل خدمة استيراد وتصدير المنتجات عبر ملفات إكسيل بنطاق Scoped
        services.AddScoped<IProductExcelService, ProductExcelService>();

        // تسجيل خدمة ربط المنتجات بوحدات القياس بنطاق Scoped
        services.AddScoped<IProductUnitService, ProductUnitService>();

        // تسجيل خدمة إدارة باركودات المنتجات بنطاق Scoped
        services.AddScoped<IProductBarCodeService, ProductBarCodeService>();

        // تسجيل خدمة إدارة صور المنتجات بنطاق Scoped
        services.AddScoped<IProductImageService, ProductImageService>();

        // تسجيل خدمة إدارة بيانات العملاء بنطاق Scoped
        services.AddScoped<ICustomerService, CustomerService>();

        // تسجيل خدمة فواتير المبيعات بنطاق Scoped
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();

        // تسجيل خدمة مرتجعات المبيعات بنطاق Scoped
        services.AddScoped<ISalesReturnService, SalesReturnService>();

        // تسجيل خدمة إدارة الموردين بنطاق Scoped
        services.AddScoped<ISupplierService, SupplierService>();

        // تسجيل خدمة إدارة المستودعات بنطاق Scoped
        services.AddScoped<IWarehouseService, WarehouseService>();

        // تسجيل خدمة إدارة أرصدة المخزون وحركاته بنطاق Scoped
        services.AddScoped<IStockService, StockService>();

        // تسجيل خدمة تحويلات المخزون بين المستودعات بنطاق Scoped
        services.AddScoped<IStockTransferService, StockTransferService>();

        // تسجيل خدمة تسويات وجرد المخزون بنطاق Scoped
        services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();

        // تسجيل خدمة أوامر الشراء بنطاق Scoped
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

        // تسجيل خدمة فواتير الشراء بنطاق Scoped
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

        // تسجيل خدمة مرتجعات الشراء بنطاق Scoped
        services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();

        // تسجيل خدمة رفع وتخزين الملفات بنطاق Scoped
        services.AddScoped<IFileUploadService, FileUploadService>();

        // تسجيل خدمة لوحة التحكم والإحصائيات بنطاق Scoped
        services.AddScoped<IDashboardService, DashboardService>();

        // تسجيل خدمة إدارة حسابات المستخدمين وصلاحياتهم بنطاق Scoped
        services.AddScoped<RetalSystemAPI.Services.Users.Interfaces.IUserService, RetalSystemAPI.Services.Users.Implementations.UserService>();

        // تسجيل خدمة التحقق من الصلاحيات للمستخدم الحالي بنطاق Scoped
        services.AddScoped<RetalSystemAPI.Services.Common.Interfaces.ICurrentPermissionService, RetalSystemAPI.Services.Common.Implementations.CurrentPermissionService>();

        // إرجاع حاوية الخدمات بعد اكتمال التسجيل
        return services;
    }
}
