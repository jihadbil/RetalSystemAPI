using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Services.Catalog.Specifications;

/// <summary>
/// مواصفة جلب صنف محدد مع كافة تفاصيله الدقيقة (التصنيف، الوحدات، الباركودات وصورها، وصور المنتج).
/// </summary>
public class ProductWithDetailsSpec : BaseSpecification<Product>
{
    /// <summary>تهيئة مواصفة الصنف المفصل بالمعرف</summary>
    /// <param name="id">معرف الصنف</param>
    public ProductWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        AddInclude(p => p.Category!);
        AddInclude("ProductUnits.Unit");
        AddInclude("ProductBarCodes.ProductImages");
        AddInclude(p => p.ProductImages);
    }
}

/// <summary>
/// مواصفة جلب ملخصات الأصناف مع إمكانية الفلترة بتصنيف معين.
/// </summary>
public class ProductSummarySpec : BaseSpecification<Product>
{
    /// <summary>تهيئة مواصفة ملخص الأصناف</summary>
    /// <param name="categoryId">معرف التصنيف الاختياري</param>
    public ProductSummarySpec(Guid? categoryId = null) 
        : base(p => !categoryId.HasValue || p.CategoryId == categoryId.Value)
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.ProductBarCodes);
        AddInclude(p => p.ProductImages);
        AddInclude("ProductUnits.Unit");
        ApplyOrderBy(p => p.Name);
    }
}

/// <summary>
/// مواصفة البحث السريع في الأصناف بالاسم أو الباركود.
/// </summary>
public class ProductSearchSpec : BaseSpecification<Product>
{
    /// <summary>تهيئة مواصفة البحث</summary>
    /// <param name="query">كلمة البحث</param>
    public ProductSearchSpec(string query)
        : base(p => p.Name.Contains(query) || p.ProductBarCodes.Any(b => b.BarCode.Contains(query)))
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.ProductBarCodes);
        AddInclude(p => p.ProductImages);
        AddInclude("ProductUnits.Unit");
        ApplyOrderBy(p => p.Name);
    }
}
