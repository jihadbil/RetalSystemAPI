using AutoMapper;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class PurchaseMappingProfile : Profile
{
    public PurchaseMappingProfile()
    {
        CreateMap<PurchaseOrder, PurchaseOrderResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src =>
                src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseOrder, PurchaseOrderSummaryDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src =>
                src.Warehouse != null ? src.Warehouse.Name : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => PurchaseOrderStatus.Draft))
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

        CreateMap<PurchaseOrderItem, PurchaseOrderItemResponseDto>()
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null!))
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null!))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductBarCode != null && src.ProductBarCode.Product != null ? src.ProductBarCode.Product.Name : null!));

        CreateMap<PurchaseOrderItemDto, PurchaseOrderItem>()
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore());

        // ── Purchase Invoice Mappings ─────────────────────────
        CreateMap<PurchaseInvoice, PurchaseInvoiceSummaryDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<PurchaseInvoice, PurchaseInvoiceResponseDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.OrderNumber : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseInvoiceItem, PurchaseInvoiceItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.BarCode, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        CreateMap<CreatePurchaseInvoiceDto, PurchaseInvoice>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<CreatePurchaseInvoiceItemDto, PurchaseInvoiceItem>();
    }
}
