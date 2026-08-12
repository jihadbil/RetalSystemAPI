using System;

namespace RetalSystemAPI.Desktop.Models.Warehouses;

public enum WarehouseType
{
    Storge = 1,
    Show = 2
}

public class WarehouseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public WarehouseType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class WarehouseSummaryDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public WarehouseType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateWarehouseRequest
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public WarehouseType Type { get; set; }
}

public class UpdateWarehouseRequest
{
    public string Name { get; set; } = null!;
    public WarehouseType Type { get; set; }
}

public class StorgeStockDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public Guid ProductBarcodeId { get; set; }
    public string BarcodeTitle { get; set; } = null!;
    public string BarcodeValue { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; }
    public bool IsBelowMinLevel { get; set; }
}

public class SetStorgeStockRequest
{
    public Guid WarehouseId { get; set; }
    public Guid ProductBarcodeId { get; set; }
    public decimal Quantity { get; set; }
    public int MinStockLevel { get; set; }
}

public class ShowroomStockDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; }
    public bool IsBelowMinLevel { get; set; }
}

public class SetShowroomStockRequest
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public int MinStockLevel { get; set; }
}
