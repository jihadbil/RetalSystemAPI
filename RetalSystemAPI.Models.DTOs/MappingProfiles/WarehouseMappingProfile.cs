using AutoMapper;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class WarehouseMappingProfile : Profile
{
    public WarehouseMappingProfile()
    {
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
    }
}
