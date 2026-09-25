using AdsConnect.data.Dtos;
using AdsConnect.data.Model;
using AutoMapper;

namespace AdsConnect.data.MappingClass
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Industry, IndustryDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.IndustryId))
                .ForMember(d => d.name, o => o.MapFrom(s => s.IndustryName));

            CreateMap<Location, LocationDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.LocationId))
                .ForMember(d => d.city, o => o.MapFrom(s => s.City))
                .ForMember(d => d.state, o => o.MapFrom(s => s.State));

            CreateMap<ProviderType, ProviderTypeDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.ProviderTypeId))
                .ForMember(d => d.name, o => o.MapFrom(s => s.Name));

            CreateMap<AdvertisingChannel, ChannelDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.ChannelId))
                .ForMember(d => d.name, o => o.MapFrom(s => s.ChannelName))
                .ForMember(d => d.category, o => o.MapFrom(s => s.Category))
                .ForMember(d => d.description, o => o.MapFrom(s => s.Description));

            CreateMap<PricingUnit, PricingUnitDto>()
                .ForMember(d => d.id, o => o.MapFrom(s => s.PricingUnitId))
                .ForMember(d => d.code, o => o.MapFrom(s => s.UnitCode))
                .ForMember(d => d.name, o => o.MapFrom(s => s.UnitName));

            // Provider has no Provider -> ProviderDto map on purpose: a card is a
            // flatten across Provider, ProviderType, Location, AdvertisingChannel and
            // ProviderPricing, which ProviderService builds as a single SQL projection.
            // Mapping it here would force loading whole graphs per provider instead.
        }
    }
}
