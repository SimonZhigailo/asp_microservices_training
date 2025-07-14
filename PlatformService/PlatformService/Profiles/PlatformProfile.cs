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
        }
    }
}