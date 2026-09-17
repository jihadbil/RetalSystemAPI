using AutoMapper;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات الموردين (Supplier Mapping Profile).
/// يحدد قواعد التحويل بين كيانات الموردين وهواتفهم ونواقل البيانات في كلا الاتجاهين.
/// </summary>
public class SupplierMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات الموردين ونماذج الإنشاء والتعديل والاستجابة وهواتف الموردين.
    /// </summary>
    public SupplierMappingProfile()
    {
        // تكوين تحويل كيان المورد إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<Supplier, SupplierResponseDto>()
            // إسقاط قائمة هواتف المورد من الخاصية الملاحية SupplierPhones
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.SupplierPhones));

        // تكوين تحويل كيان المورد إلى ناقل بيانات الملخص السريع للعرض في الجداول
        CreateMap<Supplier, SupplierSummaryDto>()
            // احتساب إجمالي عدد الهواتف المسجلة للمورد
            .ForMember(dest => dest.PhoneCount, opt => opt.MapFrom(src => src.SupplierPhones.Count));

        // تكوين تحويل ناقل بيانات إنشاء المورد إلى كيان المورد
        CreateMap<CreateSupplierDto, Supplier>();

        // تكوين تحويل ناقل بيانات تعديل المورد إلى كيان المورد
        CreateMap<UpdateSupplierDto, Supplier>()
            // استبعاد المعرف الأساسي Id لمنع تعديل المفتاح الأساسي للكيان
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // تكوين تحويل كيان هاتف المورد إلى ناقل بيانات استجابة هاتف المورد
        CreateMap<SupplierPhone, SupplierPhoneResponseDto>();
        // تكوين تحويل ناقل بيانات هاتف المورد إلى كيان هاتف المورد
        CreateMap<SupplierPhoneDto, SupplierPhone>();
    }
}
