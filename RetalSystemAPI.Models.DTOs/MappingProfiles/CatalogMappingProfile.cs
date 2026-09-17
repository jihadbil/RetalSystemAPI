using AutoMapper;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات كتالوج المنتجات (Catalog Mapping Profile).
/// يحدد قواعد التحويل بين كيانات التصنيفات والوحدات والمنتجات والباركودات والصور ونواقل البيانات المقابلة لها.
/// </summary>
public class CatalogMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات الكتالوج (الأصناف، الفئات، الوحدات، الباركود، والصور) إلى نواقل البيانات والعكس.
    /// </summary>
    public CatalogMappingProfile()
    {
        // ── تحويلات فئات وتصنيفات الأصناف (Category) ─────────────────────────
        // تكوين تحويل كيان الفئة إلى ناقل بيانات الاستجابة
        CreateMap<Category, CategoryResponseDto>()
            // إسقاط اسم الفئة الأب إن وجدت أو تعيين قيمة فارغة
            .ForMember(dest => dest.ParentCategoryName,
                opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            // احتساب إجمالي عدد الأصناف المرتبطة بهذه الفئة
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count));

        // تكوين تحويل ناقل بيانات إنشاء الفئة إلى كيان الفئة
        CreateMap<CreateCategoryDto, Category>();
        // تكوين تحويل ناقل بيانات تحديث الفئة إلى كيان الفئة
        CreateMap<UpdateCategoryDto, Category>()
            // استبعاد المعرف Id من التعديل للحفاظ على هوية السجل
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ── تحويلات وحدات القياس (Unit) ──────────────────────────────────────
        // تكوين تحويل كيان الوحدة إلى ناقل بيانات استجابة الوحدة
        CreateMap<Unit, UnitResponseDto>();
        // تكوين تحويل ناقل بيانات إنشاء الوحدة إلى كيان الوحدة
        CreateMap<CreateUnitDto, Unit>();
        // تكوين تحويل ناقل بيانات تحديث الوحدة إلى كيان الوحدة
        CreateMap<UpdateUnitDto, Unit>()
            // استبعاد المعرف Id من التعديل
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ── تحويلات باركودات الأصناف (ProductBarCode) ─────────────────────────
        // تكوين تحويل كيان باركود الصنف إلى ناقل بيانات الاستجابة
        CreateMap<ProductBarCode, ProductBarCodeResponseDto>()
            // إسقاط قائمة الصور المرتبطة بالباركود
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages))
            // إسقاط اسم الصنف المرتبط بهذا الباركود بأمان مع التحقق من عدم الفراغ
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط سعر التكلفة من كيان الصنف المرتبط
            .ForMember(dest => dest.CostPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.CostPrice : 0m));
        // تكوين تحويل ناقل بيانات إنشاء الباركود إلى كيان الباركود
        CreateMap<CreateProductBarCodeDto, ProductBarCode>();

        // ── تحويلات صور الأصناف (ProductImage) ────────────────────────────────
        // تكوين تحويل كيان صورة الصنف إلى ناقل بيانات استجابة الصورة
        CreateMap<ProductImage, ProductImageResponseDto>();
        // تكوين تحويل ناقل بيانات إنشاء الصورة إلى كيان الصورة
        CreateMap<CreateProductImageDto, ProductImage>();

        // ── تحويلات وحدات الصنف (ProductUnit) ─────────────────────────────────
        // تكوين تحويل كيان وحدة الصنف إلى ناقل بيانات الاستجابة
        CreateMap<ProductUnit, ProductUnitResponseDto>()
            // إسقاط اسم الوحدة من الكيان المرتبط Unit
            .ForMember(dest => dest.UnitName,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
        // تكوين تحويل ناقل بيانات ربط وحدة بصنف إلى كيان وحدة الصنف
        CreateMap<CreateProductUnitDto, ProductUnit>();

        // ── تحويلات المنتجات والأصناف (Product) ──────────────────────────────
        // تكوين تحويل كيان الصنف إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<Product, ProductResponseDto>()
            // إسقاط اسم التصنيف الرئيسي
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            // إسقاط وحدات القياس المتعددة للصنف
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.ProductUnits))
            // إسقاط قائمة الباركودات المعرفة للصنف
            .ForMember(dest => dest.BarCodes, opt => opt.MapFrom(src => src.ProductBarCodes))
            // إسقاط قائمة صور الصنف
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages));

        // تكوين تحويل كيان الصنف إلى ناقل بيانات الملخص السريع لعرض الجداول
        CreateMap<Product, ProductSummaryDto>()
            // إسقاط اسم التصنيف
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            // استخراج الصورة الافتراضية للصنف أو أول صورة مسجلة
            .ForMember(dest => dest.DefaultImage,
                opt => opt.MapFrom(src =>
                    src.ProductImages.FirstOrDefault(i => i.IsDefault) != null
                        ? src.ProductImages.First(i => i.IsDefault).ImageUrl
                        : src.ProductImages.FirstOrDefault() != null
                            ? src.ProductImages.First().ImageUrl
                            : null))
            // استخراج الباركود الافتراضي الأساسي للصنف
            .ForMember(dest => dest.DefaultBarCode,
                opt => opt.MapFrom(src =>
                    src.ProductBarCodes.FirstOrDefault() != null
                        ? src.ProductBarCodes.First().BarCode
                        : null))
            // إسقاط قائمة الباركودات
            .ForMember(dest => dest.BarCodes, opt => opt.MapFrom(src => src.ProductBarCodes))
            // إسقاط قائمة الوحدات
            .ForMember(dest => dest.Units, opt => opt.MapFrom(src => src.ProductUnits))
            // إسقاط قائمة الصور
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages));

        // تكوين تحويل ناقل بيانات إنشاء الصنف إلى كيان الصنف
        CreateMap<CreateProductDto, Product>();
        // تكوين تحويل ناقل بيانات تعديل الصنف إلى كيان الصنف
        CreateMap<UpdateProductDto, Product>()
            // استبعاد المعرف الأساسي Id من عملية التعديل
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
