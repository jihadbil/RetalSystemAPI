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
    }
}
