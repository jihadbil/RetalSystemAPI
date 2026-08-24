using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Catalog;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryDto> Children { get; set; } = new();
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}

public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}

public class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}

public class UpdateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? DefaultImageUrl { get; set; }
    public string? DefaultImage { get; set; }
    public string? DefaultBarCode { get; set; }
    public List<ProductUnitDto> Units { get; set; } = new();
    public List<ProductBarCodeDto> BarCodes { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public int InitialShowroomQuantity { get; set; } = 0;
    public Guid? ShowroomWarehouseId { get; set; }
    public Guid? StorageWarehouseId { get; set; }
    public List<CreateShowroomStockQuantityRequest> ShowroomInitialQuantities { get; set; } = new();
    public List<CreateStorageStockQuantityRequest> StorageInitialQuantities { get; set; } = new();
    public List<CreateProductBarCodeRequest> BarCodes { get; set; } = new();
    public List<CreateProductUnitRequest> Units { get; set; } = new();
}

public class CreateShowroomStockQuantityRequest
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CreateStorageStockQuantityRequest
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string BarCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public List<CreateProductBarCodeRequest> BarCodes { get; set; } = new();
    public List<CreateProductUnitRequest> Units { get; set; } = new();
}

public class ProductUnitDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int ConversionFactor { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateProductUnitRequest
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int ConversionFactor { get; set; } = 1;
    public bool IsDefault { get; set; }
}

public class ProductBarCodeDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
}

public class CreateProductBarCodeRequest
{
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int InitialQuantity { get; set; } = 0;
}

public class UpdateProductBarCodeRequest
{
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public Guid? BarcodeId { get; set; }
    public string BarcodeTitle { get; set; } = string.Empty;

    public string FullImageUrl => string.IsNullOrWhiteSpace(ImageUrl)
        ? string.Empty
        : (ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? ImageUrl
            : $"https://localhost:7226/{ImageUrl.TrimStart('/')}");
}

public class ProductImportResultDto
{
    public int CategoriesImported { get; set; }
    public int ProductsImported { get; set; }
    public int BarcodesImported { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => Errors.Count == 0;
}
