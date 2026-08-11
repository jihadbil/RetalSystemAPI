using AutoMapper;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class SupplierMappingProfile : Profile
{
    public SupplierMappingProfile()
    {
        CreateMap<Supplier, SupplierResponseDto>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.SupplierPhones));

        CreateMap<Supplier, SupplierSummaryDto>()
            .ForMember(dest => dest.PhoneCount, opt => opt.MapFrom(src => src.SupplierPhones.Count));

        CreateMap<CreateSupplierDto, Supplier>();

        CreateMap<UpdateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<SupplierPhone, SupplierPhoneResponseDto>();
        CreateMap<SupplierPhoneDto, SupplierPhone>();
    }
}
