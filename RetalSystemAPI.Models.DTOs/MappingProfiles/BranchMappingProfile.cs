using AutoMapper;
using BranchEntity = RetalSystemAPI.Models.Branchs.Branch;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.DTOs.Branch;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات الفروع (Branch Mapping Profile).
/// يحدد قواعد التحويل بين كيانات الفروع وهواتفها ونواقل البيانات المقابلة لها في كلا الاتجاهين.
/// </summary>
public class BranchMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد إسقاط وتحويل البيانات الخاصة بالفروع وهواتف الفروع.
    /// </summary>
    public BranchMappingProfile()
    {
        // تكوين تحويل كيان الفرع إلى ناقل بيانات استجابة الفرع
        CreateMap<BranchEntity, BranchResponseDto>()
            // إسقاط قائمة هواتف الفرع من الخاصية الملاحية BranchPhones
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.BranchPhones));

        // تكوين تحويل كيان هاتف الفرع إلى ناقل بيانات استجابة هاتف الفرع
        CreateMap<BranchPhone, BranchPhoneResponseDto>();

        // تكوين تحويل ناقل بيانات إنشاء الفرع إلى كيان الفرع
        CreateMap<CreateBranchDto, BranchEntity>()
            // إسقاط قائمة أرقام الهواتف الواردة في الطلب إلى خاصية الكيان BranchPhones
            .ForMember(dest => dest.BranchPhones, opt => opt.MapFrom(src => src.Phones));

        // تكوين تحويل ناقل بيانات هاتف الفرع إلى كيان هاتف الفرع
        CreateMap<BranchPhoneDto, BranchPhone>();

        // تكوين تحويل ناقل بيانات تعديل الفرع إلى كيان الفرع
        CreateMap<UpdateBranchDto, BranchEntity>()
            // استبعاد المعرف Id من عملية التعديل لمنع تعديل المفتاح الأساسي للكيان
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
