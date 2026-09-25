using System.Text.RegularExpressions;
using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class ReferenceService : IReferenceService
    {
        private static readonly Regex CheckConstraintValue =
            new(@"'([^']+)'::character varying", RegexOptions.Compiled);

        private readonly AdsConnectContext _adsConnectContext;
        private readonly IMapper _mapper;

        public ReferenceService(AdsConnectContext adsConnectContext, IMapper mapper)
        {
            _adsConnectContext = adsConnectContext;
            _mapper = mapper;
        }

        public async Task<ReferenceDataDto> GetReferenceDataService()
        {
            try
            {
                var industries = await _adsConnectContext.Industries
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.IndustryName)
                    .ToListAsync();

                var locations = await _adsConnectContext.Locations
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.City)
                    .ToListAsync();

                var providerTypes = await _adsConnectContext.ProviderTypes
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
                    .ToListAsync();

                var channels = await _adsConnectContext.AdvertisingChannels
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();

                var pricingUnits = await _adsConnectContext.PricingUnits
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.UnitName)
                    .ToListAsync();

                return new ReferenceDataDto
                {
                    industries = _mapper.Map<List<IndustryDto>>(industries),
                    locations = _mapper.Map<List<LocationDto>>(locations),
                    providerTypes = _mapper.Map<List<ProviderTypeDto>>(providerTypes),
                    channels = _mapper.Map<List<ChannelDto>>(channels),
                    pricingUnits = _mapper.Map<List<PricingUnitDto>>(pricingUnits),
                    objectives = await GetObjectives(),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<LookupsBundleDto> GetLookupsBundleService()
        {
            var channels = await _adsConnectContext.AdvertisingChannels
                .Select(c => new LookupItemDto
                {
                    id = c.ChannelId,
                    name = c.ChannelName,
                    category = c.Category,
                    description = c.Description,
                    isActive = c.IsActive,
                    itemCount = c.ProviderChannels.Count + c.CampaignRequirements.Count,
                })
                .OrderBy(c => c.name)
                .ToListAsync();

            var industries = await _adsConnectContext.Industries
                .Select(i => new LookupItemDto
                {
                    id = i.IndustryId,
                    name = i.IndustryName,
                    isActive = i.IsActive,
                    itemCount = i.Advertisers.Count + i.Campaigns.Count,
                })
                .OrderBy(i => i.name)
                .ToListAsync();

            var locations = await _adsConnectContext.Locations
                .Select(l => new LookupItemDto
                {
                    id = l.LocationId,
                    name = l.City + (string.IsNullOrEmpty(l.State) ? "" : $", {l.State}"),
                    category = l.Country,
                    description = l.State,
                    isActive = l.IsActive,
                    itemCount = l.Advertisers.Count + l.Providers.Count + l.ProviderLocations.Count,
                })
                .OrderBy(l => l.name)
                .ToListAsync();

            var providerTypes = await _adsConnectContext.ProviderTypes
                .Select(p => new LookupItemDto
                {
                    id = p.ProviderTypeId,
                    name = p.Name,
                    description = p.Description,
                    isActive = p.IsActive,
                    itemCount = p.Providers.Count,
                })
                .OrderBy(p => p.name)
                .ToListAsync();

            var pricingUnits = await _adsConnectContext.PricingUnits
                .Select(u => new LookupItemDto
                {
                    id = u.PricingUnitId,
                    name = u.UnitName,
                    code = u.UnitCode,
                    description = u.IsTimeBased ? "Time Based Unit" : "Post / Performance Unit",
                    isActive = u.IsActive,
                    itemCount = u.ProviderPricings.Count + u.ProposalItems.Count,
                })
                .OrderBy(u => u.name)
                .ToListAsync();

            var roles = await _adsConnectContext.Roles
                .Select(r => new LookupItemDto
                {
                    id = r.RoleId,
                    name = r.RoleName,
                    description = r.Description,
                    isActive = r.IsActive,
                    itemCount = r.UserRoles.Count,
                })
                .OrderBy(r => r.name)
                .ToListAsync();

            return new LookupsBundleDto
            {
                channels = channels,
                industries = industries,
                locations = locations,
                providerTypes = providerTypes,
                pricingUnits = pricingUnits,
                roles = roles,
            };
        }

        public async Task<(bool success, string message)> CreateLookupItemService(CreateLookupItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.name))
            {
                return (false, "Name is required.");
            }

            var cat = (dto.category ?? "").Trim().ToLowerInvariant();

            switch (cat)
            {
                case "channels":
                    var channel = new Model.AdvertisingChannel
                    {
                        ChannelId = Guid.NewGuid(),
                        ChannelName = dto.name.Trim(),
                        Category = string.IsNullOrWhiteSpace(dto.subCategory) ? "Digital" : dto.subCategory.Trim(),
                        Description = dto.description?.Trim() ?? $"{dto.name} channel",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _adsConnectContext.AdvertisingChannels.Add(channel);
                    break;

                case "industries":
                    var industry = new Model.Industry
                    {
                        IndustryId = Guid.NewGuid(),
                        IndustryName = dto.name.Trim(),
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _adsConnectContext.Industries.Add(industry);
                    break;

                case "locations":
                    var location = new Model.Location
                    {
                        LocationId = Guid.NewGuid(),
                        City = dto.name.Trim(),
                        State = dto.state?.Trim() ?? (dto.subCategory?.Trim() ?? "General"),
                        Country = "India",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _adsConnectContext.Locations.Add(location);
                    break;

                case "providertypes":
                    var providerType = new Model.ProviderType
                    {
                        ProviderTypeId = Guid.NewGuid(),
                        Name = dto.name.Trim(),
                        Description = dto.description?.Trim() ?? $"{dto.name} creator profile",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _adsConnectContext.ProviderTypes.Add(providerType);
                    break;

                case "pricingunits":
                    var code = string.IsNullOrWhiteSpace(dto.code)
                        ? dto.name.Trim().ToLowerInvariant().Replace(" ", "_")
                        : dto.code.Trim();
                    var pricingUnit = new Model.PricingUnit
                    {
                        PricingUnitId = Guid.NewGuid(),
                        UnitName = dto.name.Trim(),
                        UnitCode = code,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _adsConnectContext.PricingUnits.Add(pricingUnit);
                    break;

                case "roles":
                    var role = new Model.Role
                    {
                        RoleId = Guid.NewGuid(),
                        RoleName = dto.name.Trim(),
                        Description = dto.description?.Trim() ?? $"{dto.name} system role",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _adsConnectContext.Roles.Add(role);
                    break;

                default:
                    return (false, $"Unsupported category: {dto.category}");
            }

            await _adsConnectContext.SaveChangesAsync();
            return (true, $"{dto.name} created successfully.");
        }

        public async Task<(bool success, string message)> ToggleLookupStatusService(ToggleLookupStatusDto dto)
        {
            var cat = (dto.category ?? "").Trim().ToLowerInvariant();

            switch (cat)
            {
                case "channels":
                    var channel = await _adsConnectContext.AdvertisingChannels.FindAsync(dto.id);
                    if (channel == null) return (false, "Channel not found.");
                    channel.IsActive = !channel.IsActive;
                    break;

                case "industries":
                    var industry = await _adsConnectContext.Industries.FindAsync(dto.id);
                    if (industry == null) return (false, "Industry not found.");
                    industry.IsActive = !industry.IsActive;
                    break;

                case "locations":
                    var location = await _adsConnectContext.Locations.FindAsync(dto.id);
                    if (location == null) return (false, "Location not found.");
                    location.IsActive = !location.IsActive;
                    break;

                case "providertypes":
                    var providerType = await _adsConnectContext.ProviderTypes.FindAsync(dto.id);
                    if (providerType == null) return (false, "Provider type not found.");
                    providerType.IsActive = !providerType.IsActive;
                    break;

                case "pricingunits":
                    var pricingUnit = await _adsConnectContext.PricingUnits.FindAsync(dto.id);
                    if (pricingUnit == null) return (false, "Pricing unit not found.");
                    pricingUnit.IsActive = !pricingUnit.IsActive;
                    break;

                case "roles":
                    var role = await _adsConnectContext.Roles.FindAsync(dto.id);
                    if (role == null) return (false, "Role not found.");
                    role.IsActive = !role.IsActive;
                    break;

                default:
                    return (false, $"Unsupported category: {dto.category}");
            }

            await _adsConnectContext.SaveChangesAsync();
            return (true, "Status updated successfully.");
        }

        private async Task<List<string>> GetObjectives()
        {
            var definitions = await _adsConnectContext.Database
                .SqlQueryRaw<string>(@"
                    SELECT pg_get_constraintdef(con.oid) AS ""Value""
                    FROM pg_constraint con
                    JOIN pg_class rel ON rel.oid = con.conrelid
                    WHERE rel.relname = 'Campaign' AND con.conname = 'CK_Campaign_Objective'")
                .ToListAsync();

            var definition = definitions.FirstOrDefault();
            if (string.IsNullOrEmpty(definition))
            {
                return new List<string> { "Awareness", "Conversion", "Engagement", "Traffic", "Lead Generation" };
            }

            return CheckConstraintValue.Matches(definition)
                .Select(m => m.Groups[1].Value)
                .ToList();
        }
    }
}
