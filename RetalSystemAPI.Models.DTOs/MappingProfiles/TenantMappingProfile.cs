using AutoMapper;
using TenantEntity = RetalSystemAPI.Models.Tenant;
using RetalSystemAPI.Models.DTOs.Tenant;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات المستأجرين (Tenant Mapping Profile).
/// يحدد قواعد التحويل بين كيان المستأجر ونواقل بيانات الإنشاء والتعديل والاستجابة.
/// </summary>
public class TenantMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيان المستأجر إلى نواقل البيانات والعكس.
    /// </summary>
    public TenantMappingProfile()
    {
        // تكوين تحويل كيان المستأجر إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<TenantEntity, TenantResponseDto>();

        // تكوين تحويل ناقل بيانات إنشاء المستأجر إلى كيان المستأجر
        CreateMap<CreateTenantDto, TenantEntity>();

        // تكوين تحويل ناقل بيانات تعديل المستأجر إلى كيان المستأجر
        CreateMap<UpdateTenantDto, TenantEntity>()
            // استبعاد المعرف الأساسي Id لمنع تعديل المفتاح الأساسي للمستأجر
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
