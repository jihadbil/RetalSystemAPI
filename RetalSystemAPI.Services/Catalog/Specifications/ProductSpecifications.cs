using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Services.Catalog.Specifications;

/// <summary>
/// تخصيص استعلامات المنتجات مع علاقاتها المختلفة (ProductUnits, BarCodes, Images, Category).
/// </summary>
public class ProductWithDetailsSpec : BaseSpecification<Product>
{
    public ProductWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        AddInclude(p => p.Category!);
        AddInclude("ProductUnits.Unit");
        AddInclude("ProductBarCodes.ProductImages");
        AddInclude(p => p.ProductImages);
    }
}

public class ProductSummarySpec : BaseSpecification<Product>
{
    public ProductSummarySpec(Guid? categoryId = null) 
        : base(p => !categoryId.HasValue || p.CategoryId == categoryId.Value)
    {
        AddInclude(p => p.Category!);
        ApplyOrderBy(p => p.Name);
    }
}

public class ProductSearchSpec : BaseSpecification<Product>
{
    public ProductSearchSpec(string query) 
        : base(p => p.Name.Contains(query) || p.ProductBarCodes.Any(b => b.BarCode.Contains(query)))
    {
        AddInclude(p => p.Category!);
        ApplyOrderBy(p => p.Name);
    }
}
