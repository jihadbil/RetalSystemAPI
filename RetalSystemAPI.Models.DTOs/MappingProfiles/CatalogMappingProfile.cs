using AutoMapper;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // Category
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(dest => dest.ParentCategoryName,
                opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count));

        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Unit
        CreateMap<Unit, UnitResponseDto>();
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ProductBarCode
        CreateMap<ProductBarCode, ProductBarCodeResponseDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages));
        CreateMap<CreateProductBarCodeDto, ProductBarCode>();

        // ProductImage
        CreateMap<ProductImage, ProductImageResponseDto>();
        CreateMap<CreateProductImageDto, ProductImage>();

        // ProductUnit
        CreateMap<ProductUnit, ProductUnitResponseDto>()
            .ForMember(dest => dest.UnitName,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
        CreateMap<CreateProductUnitDto, ProductUnit>();

        // Product
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.ProductUnits))
            .ForMember(dest => dest.BarCodes, opt => opt.MapFrom(src => src.ProductBarCodes))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages));

        CreateMap<Product, ProductSummaryDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.DefaultImage,
                opt => opt.MapFrom(src =>
                    src.ProductImages.FirstOrDefault(i => i.IsDefault) != null
                        ? src.ProductImages.First(i => i.IsDefault).ImageUrl
                        : src.ProductImages.FirstOrDefault() != null
                            ? src.ProductImages.First().ImageUrl
                            : null))
            .ForMember(dest => dest.DefaultBarCode,
                opt => opt.MapFrom(src =>
                    src.ProductBarCodes.FirstOrDefault() != null
                        ? src.ProductBarCodes.First().BarCode
                        : null));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
