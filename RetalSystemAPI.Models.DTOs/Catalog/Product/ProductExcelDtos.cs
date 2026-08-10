using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// نموذج بيانات الصنف المستورد أو المصدر عبر أكسل (الورقة الأولى).
/// </summary>
public class ProductExcelRowDto
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// نموذج بيانات الباركود المستورد أو المصدر عبر أكسل (الورقة الثانية).
/// </summary>
public class BarcodeExcelRowDto
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public required string BarCode { get; set; }
    public string? Title { get; set; }
}

/// <summary>
/// نموذج بيانات التصنيف المستورد أو المصدر عبر أكسل (الورقة الثالثة).
/// </summary>
public class CategoryExcelRowDto
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
}

/// <summary>
/// نتيجة عملية استيراد الأصناف من ملف أكسل.
/// </summary>
public class ProductImportResultDto
{
    public int CategoriesImported { get; set; }
    public int ProductsImported { get; set; }
    public int BarcodesImported { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => Errors.Count == 0;
}
