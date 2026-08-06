using AutoMapper;
using TenantEntity = RetalSystemAPI.Models.Tenant;
using RetalSystemAPI.Models.DTOs.Tenant;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<TenantEntity, TenantResponseDto>();
        CreateMap<CreateTenantDto, TenantEntity>();
        CreateMap<UpdateTenantDto, TenantEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
