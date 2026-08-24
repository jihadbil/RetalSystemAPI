using AutoMapper;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerResponseDto>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.CustomerPhones))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == CustomerType.Retail ? "قطاعي" :
                src.Type == CustomerType.Wholesale ? "جملة" :
                src.Type == CustomerType.Corporate ? "شركات" : src.Type.ToString()));

        CreateMap<Customer, CustomerSummaryDto>()
            .ForMember(dest => dest.PhoneCount, opt => opt.MapFrom(src => src.CustomerPhones.Count))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == CustomerType.Retail ? "قطاعي" :
                src.Type == CustomerType.Wholesale ? "جملة" :
                src.Type == CustomerType.Corporate ? "شركات" : src.Type.ToString()));

        CreateMap<CreateCustomerDto, Customer>()
            .ForMember(dest => dest.CustomerPhones, opt => opt.Ignore());

        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerPhones, opt => opt.Ignore());

        CreateMap<CustomerPhone, CustomerPhoneResponseDto>();
        CreateMap<CustomerPhoneDto, CustomerPhone>();
    }
}
