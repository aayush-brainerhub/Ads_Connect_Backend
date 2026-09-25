using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class CampaignService : ICampaignService
    {
        private readonly AdsConnectContext _adsConnectContext;

        public CampaignService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<List<CampaignDto>> GetMyCampaignsService(Guid userId)
        {
            try
            {
                return await Project(
                        _adsConnectContext.Campaigns
                            .Where(c => c.Advertiser.UserId == userId)
                            .OrderByDescending(c => c.CreatedDate),
                        includeAdvertiser: false)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CampaignDto>> GetAllCampaignsService()
        {
            try
            {
                return await Project(
                        _adsConnectContext.Campaigns.OrderByDescending(c => c.CreatedDate),
                        includeAdvertiser: true)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool created, string message, CampaignDto? campaign)> CreateCampaignService(
            Guid userId, CreateCampaignDto dto)
        {
            try
            {
                var title = (dto.title ?? string.Empty).Trim();
                if (title.Length == 0)
                {
                    return (false, "A campaign title is required.", null);
                }

                if (dto.budget < 0)
                {
                    return (false, "The budget cannot be negative.", null);
                }

                var advertiserId = await _adsConnectContext.Advertisers
                    .Where(a => a.UserId == userId)
                    .Select(a => (Guid?)a.AdvertiserId)
                    .FirstOrDefaultAsync();

                if (advertiserId == null)
                {
                    return (false, "Finish advertiser onboarding before creating a campaign.", null);
                }

                // CK_Campaign_Objective only accepts the eight values the reference
                // endpoint reads out of that same constraint.
                var objective = (dto.objective ?? string.Empty).Trim();
                if (!await IsValidObjective(objective))
                {
                    return (false, $"'{objective}' is not a valid campaign objective.", null);
                }

                if (dto.industryId != null &&
                    !await _adsConnectContext.Industries.AnyAsync(i => i.IndustryId == dto.industryId))
                {
                    return (false, "That industry does not exist.", null);
                }

                var campaign = new Campaign
                {
                    CampaignId = Guid.NewGuid(),
                    AdvertiserId = advertiserId.Value,
                    CampaignName = title,
                    Description = string.IsNullOrWhiteSpace(dto.description) ? null : dto.description.Trim(),
                    IndustryId = dto.industryId,
                    Objective = objective,
                    Budget = dto.budget,
                    // Created live rather than as a draft: the UI has no publish step,
                    // and a campaign nobody can be matched against is not useful.
                    Status = "Active",
                    PublishedDate = DateTime.UtcNow,
                    TargetAudience = "{}",
                };

                if (dto.locationId != null)
                {
                    // AsTracking matters here: the context defaults to NoTracking, and an
                    // untracked Location added to the skip navigation below is taken for a
                    // new row, so EF tries to INSERT it and trips Location_pkey. Tracked,
                    // it is Unchanged and only the CampaignLocation join row is written.
                    var location = await _adsConnectContext.Locations
                        .AsTracking()
                        .FirstOrDefaultAsync(l => l.LocationId == dto.locationId);

                    if (location == null)
                    {
                        return (false, "That location does not exist.", null);
                    }

                    // CampaignLocation is a pure join table with no entity class, so the
                    // link is made through the skip navigation.
                    campaign.Locations.Add(location);
                }

                _adsConnectContext.Campaigns.Add(campaign);
                await _adsConnectContext.SaveChangesAsync();

                var created = await Project(
                        _adsConnectContext.Campaigns.Where(c => c.CampaignId == campaign.CampaignId),
                        includeAdvertiser: false)
                    .FirstOrDefaultAsync();

                return (true, "Campaign created", created);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool updated, string message, CampaignDto? campaign)> UpdateCampaignService(
            Guid userId, UpdateCampaignDto dto)
        {
            try
            {
                var title = (dto.title ?? string.Empty).Trim();
                if (title.Length == 0)
                {
                    return (false, "A campaign title is required.", null);
                }

                if (dto.budget < 0)
                {
                    return (false, "The budget cannot be negative.", null);
                }

                var objective = (dto.objective ?? string.Empty).Trim();
                if (!await IsValidObjective(objective))
                {
                    return (false, $"'{objective}' is not a valid campaign objective.", null);
                }

                if (dto.industryId != null &&
                    !await _adsConnectContext.Industries.AnyAsync(i => i.IndustryId == dto.industryId))
                {
                    return (false, "That industry does not exist.", null);
                }

                var campaign = await _adsConnectContext.Campaigns
                    .Include(c => c.Locations)
                    .AsTracking()
                    .FirstOrDefaultAsync(c => c.CampaignId == dto.id && c.Advertiser.UserId == userId);

                if (campaign == null)
                {
                    return (false, "Campaign not found or you do not have permission to edit it.", null);
                }

                campaign.CampaignName = title;
                campaign.Description = string.IsNullOrWhiteSpace(dto.description) ? null : dto.description.Trim();
                campaign.IndustryId = dto.industryId;
                campaign.Objective = objective;
                campaign.Budget = dto.budget;
                campaign.UpdatedDate = DateTime.UtcNow;

                campaign.Locations.Clear();
                if (dto.locationId != null)
                {
                    var location = await _adsConnectContext.Locations
                        .AsTracking()
                        .FirstOrDefaultAsync(l => l.LocationId == dto.locationId);

                    if (location == null)
                    {
                        return (false, "That location does not exist.", null);
                    }

                    campaign.Locations.Add(location);
                }

                await _adsConnectContext.SaveChangesAsync();

                var updated = await Project(
                        _adsConnectContext.Campaigns.Where(c => c.CampaignId == campaign.CampaignId),
                        includeAdvertiser: false)
                    .FirstOrDefaultAsync();

                return (true, "Campaign updated", updated);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool deleted, string message)> DeleteCampaignService(
            Guid userId, Guid id)
        {
            try
            {
                var campaign = await _adsConnectContext.Campaigns
                    .AsTracking()
                    .FirstOrDefaultAsync(c => c.CampaignId == id && c.Advertiser.UserId == userId);

                if (campaign == null)
                {
                    return (false, "Campaign not found or you do not have permission to delete it.");
                }

                _adsConnectContext.Campaigns.Remove(campaign);
                await _adsConnectContext.SaveChangesAsync();

                return (true, "Campaign deleted");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> IsValidObjective(string objective)
        {
            if (string.IsNullOrWhiteSpace(objective)) return false;

            var allowed = await _adsConnectContext.Database
                .SqlQueryRaw<string>(@"
                    SELECT pg_get_constraintdef(con.oid) AS ""Value""
                    FROM pg_constraint con
                    JOIN pg_class rel ON rel.oid = con.conrelid
                    WHERE rel.relname = 'Campaign' AND con.conname = 'CK_Campaign_Objective'")
                .ToListAsync();

            var definition = allowed.FirstOrDefault();
            return definition != null && definition.Contains($"'{objective}'");
        }

        private IQueryable<CampaignDto> Project(IQueryable<Campaign> source, bool includeAdvertiser) =>
            source.Select(c => new CampaignDto
            {
                id = c.CampaignId,
                title = c.CampaignName,
                industry = c.Industry == null ? null : c.Industry.IndustryName,
                // A campaign can target several cities; the tables show one cell.
                location = c.Locations.Select(l => l.City).OrderBy(city => city).FirstOrDefault(),
                budget = c.Budget,
                objective = c.Objective,
                description = c.Description,
                status = c.Status,
                responses = c.ResponsesCount,
                createdAt = c.CreatedDate.ToString("yyyy-MM-dd"),
                advertiser = includeAdvertiser ? c.Advertiser.BusinessName : null,
            });
    }
}
