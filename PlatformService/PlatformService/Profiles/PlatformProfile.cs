using AutoMapper;
using PlatformService.Dto;
using PlatformService.Model;

namespace PlatformService.Platforms
{
    public class PlatformProfile : Profile
    {
        public PlatformProfile()
        {
            //source -> target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<PlatformCreateDto, Platform>();
            CreateMap<PlatformReadDto, PlatformPublishedDto>();
            CreateMap<Platform, GrpcPlatformModel>()
                .ForMember(e => e.PlatformId, opt => opt.MapFrom(src => src.Id))
                .ForMember(e => e.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(e => e.Publisher, opt => opt.MapFrom(src => src.Publisher));


        }
    }
}