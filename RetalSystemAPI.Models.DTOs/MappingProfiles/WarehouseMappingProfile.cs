using AutoMapper;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class WarehouseMappingProfile : Profile
{
    public WarehouseMappingProfile()
    {
        // ── Warehouse ─────────────────────────────────────────────
        CreateMap<Warehouse, WarehouseResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == WarehouseType.Storge ? "مخزن" : "صالة عرض"));

        CreateMap<Warehouse, WarehouseSummaryDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == WarehouseType.Storge ? "مخزن" : "صالة عرض"));

        CreateMap<CreateWarehouseDto, Warehouse>();

        CreateMap<UpdateWarehouseDto, Warehouse>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ── Stocks ────────────────────────────────────────────────
        CreateMap<StorgeStock, StorgeStockResponseDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null!))
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarcode != null ? src.ProductBarcode.Title : null!))
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarcode != null ? src.ProductBarcode.BarCode : null!))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductBarcode != null && src.ProductBarcode.Product != null ? src.ProductBarcode.Product.Name : null!))
            .ForMember(dest => dest.IsBelowMinLevel, opt => opt.MapFrom(src => src.Quantity < src.MinStockLevel));

        CreateMap<ShowroomStock, ShowroomStockResponseDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null!))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null!))
            .ForMember(dest => dest.IsBelowMinLevel, opt => opt.MapFrom(src => src.Quantity < src.MinStockLevel));

        // ── StockTransfer ─────────────────────────────────────────
        CreateMap<StockTransfer, StockTransferResponseDto>()
            .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse != null ? src.FromWarehouse.Name : string.Empty))
            .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse != null ? src.ToWarehouse.Name : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == StockTransferStatus.Draft ? "مسودة" :
                src.Status == StockTransferStatus.Confirmed ? "مؤكدة" :
                src.Status == StockTransferStatus.Completed ? "منفذة ومرحّلة" :
                src.Status == StockTransferStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<StockTransfer, StockTransferSummaryDto>()
            .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse != null ? src.FromWarehouse.Name : string.Empty))
            .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse != null ? src.ToWarehouse.Name : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == StockTransferStatus.Draft ? "مسودة" :
                src.Status == StockTransferStatus.Confirmed ? "مؤكدة" :
                src.Status == StockTransferStatus.Completed ? "منفذة ومرحّلة" :
                src.Status == StockTransferStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<CreateStockTransferDto, StockTransfer>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => StockTransferStatus.Draft))
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<UpdateStockTransferDto, StockTransfer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<StockTransferItem, StockTransferItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        CreateMap<StockTransferItemDto, StockTransferItem>();

        // ── StockAdjustment ───────────────────────────────────────
        CreateMap<StockAdjustment, StockAdjustmentResponseDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == StockAdjustmentReason.InventoryCount ? "جرد دوري" :
                src.Reason == StockAdjustmentReason.Damaged ? "بضاعة تالفة" :
                src.Reason == StockAdjustmentReason.Expired ? "بضاعة منتهية الصلاحية" :
                src.Reason == StockAdjustmentReason.InitialSetup ? "إعداد رصيد افتتاحي" :
                src.Reason == StockAdjustmentReason.Other ? "أخرى" : src.Reason.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<StockAdjustment, StockAdjustmentSummaryDto>()
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == StockAdjustmentReason.InventoryCount ? "جرد دوري" :
                src.Reason == StockAdjustmentReason.Damaged ? "بضاعة تالفة" :
                src.Reason == StockAdjustmentReason.Expired ? "بضاعة منتهية الصلاحية" :
                src.Reason == StockAdjustmentReason.InitialSetup ? "إعداد رصيد افتتاحي" :
                src.Reason == StockAdjustmentReason.Other ? "أخرى" : src.Reason.ToString()))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<CreateStockAdjustmentDto, StockAdjustment>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<StockAdjustmentItem, StockAdjustmentItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        CreateMap<StockAdjustmentItemDto, StockAdjustmentItem>()
            .ForMember(dest => dest.DifferenceQuantity, opt => opt.MapFrom(src => src.ActualQuantity - src.SystemQuantity));
    }
}
