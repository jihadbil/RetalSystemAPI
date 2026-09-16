using AutoMapper;
using BranchEntity = RetalSystemAPI.Models.Branchs.Branch;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.DTOs.Branch;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class BranchMappingProfile : Profile
{
    public BranchMappingProfile()
    {
        CreateMap<BranchEntity, BranchResponseDto>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.BranchPhones));

        CreateMap<BranchPhone, BranchPhoneResponseDto>();

        CreateMap<CreateBranchDto, BranchEntity>()
            .ForMember(dest => dest.BranchPhones, opt => opt.MapFrom(src => src.Phones));

        CreateMap<BranchPhoneDto, BranchPhone>();

        CreateMap<UpdateBranchDto, BranchEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
