using System;
using System.Linq;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Services.Catalog.Specifications;

/// <summary>
/// مواصفة جلب صنف محدد مع كافة تفاصيله الدقيقة (التصنيف، الوحدات، الباركودات وصورها، وصور المنتج).
/// </summary>
public class ProductWithDetailsSpec : BaseSpecification<Product>
{
    /// <summary>
    /// تهيئة مواصفة الصنف المفصل بالمعرف.
    /// </summary>
    /// <param name="id">معرف الصنف الفريد</param>
    public ProductWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        // تضمين بيانات تصنيف الصنف
        AddInclude(p => p.Category!);
        // تضمين وحدات القياس الخاصة بالصنف مع تعريف كل وحدة
        AddInclude("ProductUnits.Unit");
        // تضمين باركودات الصنف وصور كل باركود/نكهة
        AddInclude("ProductBarCodes.ProductImages");
        // تضمين صور المنتج العامة
        AddInclude(p => p.ProductImages);
    }
}

/// <summary>
/// مواصفة جلب ملخصات الأصناف مع إمكانية الفلترة بتصنيف معين.
/// </summary>
public class ProductSummarySpec : BaseSpecification<Product>
{
    /// <summary>
    /// تهيئة مواصفة ملخص الأصناف وترتيبها أبجدياً.
    /// </summary>
    /// <param name="categoryId">معرف التصنيف الاختياري للفلترة</param>
    public ProductSummarySpec(Guid? categoryId = null) 
        : base(p => !categoryId.HasValue || p.CategoryId == categoryId.Value)
    {
        // تضمين التصنيف التابع له المنتج
        AddInclude(p => p.Category!);
        // تضمين الباركودات المرتبطة بالمنتج
        AddInclude(p => p.ProductBarCodes);
        // تضمين الصور
        AddInclude(p => p.ProductImages);
        // تضمين الوحدات الأساسية
        AddInclude("ProductUnits.Unit");
        // ترتيب المنتجات تصاعدياً حسب الاسم
        ApplyOrderBy(p => p.Name);
    }
}

/// <summary>
/// مواصفة البحث السريع في الأصناف بالاسم أو الباركود.
/// </summary>
public class ProductSearchSpec : BaseSpecification<Product>
{
    /// <summary>
    /// تهيئة مواصفة البحث بالاسم أو أي باركود مرتبط بالمنتج.
    /// </summary>
    /// <param name="query">كلمة أو رقم البحث</param>
    public ProductSearchSpec(string query)
        : base(p => p.Name.Contains(query) || p.ProductBarCodes.Any(b => b.BarCode.Contains(query)))
    {
        // تضمين التصنيف
        AddInclude(p => p.Category!);
        // تضمين الباركودات
        AddInclude(p => p.ProductBarCodes);
        // تضمين الصور
        AddInclude(p => p.ProductImages);
        // تضمين الوحدات
        AddInclude("ProductUnits.Unit");
        // ترتيب النتائج حسب الاسم
        ApplyOrderBy(p => p.Name);
    }
}
