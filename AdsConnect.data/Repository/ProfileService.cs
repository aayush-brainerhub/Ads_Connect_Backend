using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    /// <summary>
    /// Backs the two onboarding wizards and the profile pages.
    ///
    /// Signing up writes only AppUser and UserRole, so the Advertiser / Provider
    /// row is created here on the first save. UQ_Advertiser_UserId and
    /// UQ_Provider_UserId make that at most one of each per user.
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly AdsConnectContext _adsConnectContext;

        public ProfileService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<MyProfileDto> GetMyProfileService(Guid userId)
        {
            try
            {
                return new MyProfileDto
                {
                    user = await _adsConnectContext.AppUsers
                        .Where(u => u.UserId == userId)
                        .Select(u => new AuthUserDto
                        {
                            userId = u.UserId,
                            firstName = u.FirstName,
                            lastName = u.LastName,
                            email = u.Email,
                            phoneNumber = u.PhoneNumber,
                            profileImageUrl = u.ProfileImageUrl,
                        })
                        .FirstOrDefaultAsync(),
                    advertiser = await ProjectAdvertiser(
                        _adsConnectContext.Advertisers.Where(a => a.UserId == userId)).FirstOrDefaultAsync(),
                    provider = await ProjectProvider(
                        _adsConnectContext.Providers.Where(p => p.UserId == userId)).FirstOrDefaultAsync(),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool saved, string message, string? replacedUrl)> SetProfileImageService(
            Guid userId, string url)
        {
            try
            {
                var user = await _adsConnectContext.AppUsers
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return (false, "Account not found.", null);
                }

                var previous = user.ProfileImageUrl;
                user.ProfileImageUrl = url;
                user.UpdatedDate = DateTime.UtcNow;
                await _adsConnectContext.SaveChangesAsync();

                // Reported back rather than deleted here: this layer has no business
                // touching the file system, and the row must be updated first so a
                // failed delete can never leave the column pointing at a missing file.
                return (true, "Profile picture updated", previous);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool removed, string message, string? replacedUrl)> RemoveProfileImageService(Guid userId)
        {
            try
            {
                var user = await _adsConnectContext.AppUsers
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return (false, "Account not found.", null);
                }

                var previous = user.ProfileImageUrl;
                if (previous == null)
                {
                    return (true, "No profile picture to remove", null);
                }

                user.ProfileImageUrl = null;
                user.UpdatedDate = DateTime.UtcNow;
                await _adsConnectContext.SaveChangesAsync();

                return (true, "Profile picture removed", previous);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool saved, string message, AdvertiserProfileDto? profile)> SaveAdvertiserProfileService(
            Guid userId, SaveAdvertiserProfileDto dto)
        {
            try
            {
                var businessName = (dto.businessName ?? string.Empty).Trim();
                if (businessName.Length == 0)
                {
                    return (false, "A business name is required.", null);
                }

                if (dto.monthlyBudgetMin.HasValue && dto.monthlyBudgetMax.HasValue &&
                    dto.monthlyBudgetMin > dto.monthlyBudgetMax)
                {
                    // CK_Advertiser_Budget enforces this too; catching it here gives the form a message.
                    return (false, "The minimum budget cannot exceed the maximum.", null);
                }

                var advertiser = await _adsConnectContext.Advertisers
                    .AsTracking()
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (advertiser == null)
                {
                    advertiser = new Advertiser { AdvertiserId = Guid.NewGuid(), UserId = userId, IsActive = true };
                    _adsConnectContext.Advertisers.Add(advertiser);
                }

                advertiser.BusinessName = businessName;
                advertiser.IndustryId = dto.industryId;
                advertiser.LocationId = dto.locationId;
                advertiser.Website = Trimmed(dto.website);
                advertiser.MonthlyBudgetMin = dto.monthlyBudgetMin;
                advertiser.MonthlyBudgetMax = dto.monthlyBudgetMax;

                await _adsConnectContext.SaveChangesAsync();

                var profile = await ProjectAdvertiser(
                        _adsConnectContext.Advertisers.Where(a => a.AdvertiserId == advertiser.AdvertiserId))
                    .FirstOrDefaultAsync();

                return (true, "Profile saved", profile);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool saved, string message, ProviderProfileDto? profile)> SaveProviderProfileService(
            Guid userId, SaveProviderProfileDto dto)
        {
            try
            {
                var providerName = (dto.providerName ?? string.Empty).Trim();
                if (providerName.Length == 0)
                {
                    return (false, "A provider name is required.", null);
                }

                if (!await _adsConnectContext.ProviderTypes.AnyAsync(t => t.ProviderTypeId == dto.providerTypeId && t.IsActive))
                {
                    return (false, "Choose a provider type.", null);
                }

                var provider = await _adsConnectContext.Providers
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                var isNew = provider == null;

                // The primary channel is what the discover card's category comes from,
                // so a brand new provider cannot be saved without one.
                if (isNew && dto.primaryChannelId == null)
                {
                    return (false, "Choose the channel you advertise on.", null);
                }

                if (dto.primaryChannelId != null &&
                    !await _adsConnectContext.AdvertisingChannels.AnyAsync(c => c.ChannelId == dto.primaryChannelId && c.IsActive))
                {
                    return (false, "That advertising channel does not exist.", null);
                }

                var user = await _adsConnectContext.AppUsers.AsTracking().FirstAsync(u => u.UserId == userId);

                if (provider == null)
                {
                    provider = new Provider
                    {
                        ProviderId = Guid.NewGuid(),
                        UserId = userId,
                        IsActive = true,
                        AcceptsRequests = true,
                    };
                    _adsConnectContext.Providers.Add(provider);
                }

                provider.ProviderName = providerName;
                provider.ProviderTypeId = dto.providerTypeId;
                provider.PrimaryLocationId = dto.locationId;
                provider.Email = user.Email;
                provider.PhoneNumber = Trimmed(dto.phoneNumber);
                provider.Website = Trimmed(dto.website);
                provider.ProviderDescription = Trimmed(dto.description);
                if (dto.audienceSize.HasValue)
                {
                    provider.TotalAudience = dto.audienceSize.Value;
                }

                await SavePrimaryLocation(provider.ProviderId, dto.locationId);
                await SavePrimaryChannel(provider.ProviderId, dto.primaryChannelId, dto.audienceSize);

                // The phone also lives on AppUser, which is what the header greets you by.
                if (!string.IsNullOrWhiteSpace(dto.phoneNumber))
                {
                    var phone = dto.phoneNumber.Trim();
                    var taken = await _adsConnectContext.AppUsers
                        .AnyAsync(u => u.PhoneNumber == phone && u.UserId != userId);
                    if (taken)
                    {
                        return (false, "That phone number is already registered to another account.", null);
                    }
                    user.PhoneNumber = phone;
                }

                await _adsConnectContext.SaveChangesAsync();

                var profile = await ProjectProvider(
                        _adsConnectContext.Providers.Where(p => p.ProviderId == provider.ProviderId))
                    .FirstOrDefaultAsync();

                return (true, "Profile saved", profile);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <remarks>
        /// ProviderLocation carries a PARTIAL unique index (UX_ProviderLocation_Primary,
        /// "... WHERE IsPrimary"), so there can be only one primary row. The existing
        /// one is moved rather than a second one inserted.
        /// </remarks>
        private async Task SavePrimaryLocation(Guid providerId, Guid? locationId)
        {
            var existing = await _adsConnectContext.ProviderLocations
                .AsTracking()
                .Where(l => l.ProviderId == providerId && l.IsPrimary)
                .FirstOrDefaultAsync();

            if (locationId == null)
            {
                if (existing != null) existing.IsPrimary = false;
                return;
            }

            if (existing == null)
            {
                _adsConnectContext.ProviderLocations.Add(new ProviderLocation
                {
                    ProviderLocationId = Guid.NewGuid(),
                    ProviderId = providerId,
                    LocationId = locationId.Value,
                    IsPrimary = true,
                    IsActive = true,
                });
                return;
            }

            existing.LocationId = locationId.Value;
            existing.IsActive = true;
        }

        /// <remarks>Same partial-unique-index reasoning as <see cref="SavePrimaryLocation"/>.</remarks>
        private async Task SavePrimaryChannel(Guid providerId, Guid? channelId, long? audienceSize)
        {
            if (channelId == null) return;

            var existing = await _adsConnectContext.ProviderChannels
                .AsTracking()
                .Where(c => c.ProviderId == providerId && c.IsPrimary)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                _adsConnectContext.ProviderChannels.Add(new ProviderChannel
                {
                    ProviderChannelId = Guid.NewGuid(),
                    ProviderId = providerId,
                    ChannelId = channelId.Value,
                    AudienceSize = audienceSize,
                    IsPrimary = true,
                    IsActive = true,
                    AudienceMetrics = "{}",
                });
                return;
            }

            existing.ChannelId = channelId.Value;
            existing.IsActive = true;
            if (audienceSize.HasValue) existing.AudienceSize = audienceSize;
        }

        private static string? Trimmed(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private IQueryable<AdvertiserProfileDto> ProjectAdvertiser(IQueryable<Advertiser> source) =>
            source.Select(a => new AdvertiserProfileDto
            {
                id = a.AdvertiserId,
                businessName = a.BusinessName,
                industryId = a.IndustryId,
                industry = a.Industry == null ? null : a.Industry.IndustryName,
                locationId = a.LocationId,
                location = a.Location == null ? null : a.Location.City,
                website = a.Website,
                monthlyBudgetMin = a.MonthlyBudgetMin,
                monthlyBudgetMax = a.MonthlyBudgetMax,
            });

        private IQueryable<ProviderProfileDto> ProjectProvider(IQueryable<Provider> source) =>
            source.Select(p => new ProviderProfileDto
            {
                id = p.ProviderId,
                providerName = p.ProviderName,
                providerTypeId = p.ProviderTypeId,
                providerType = p.ProviderType.Name,
                locationId = p.PrimaryLocationId,
                location = p.PrimaryLocation == null ? null : p.PrimaryLocation.City,
                phoneNumber = p.PhoneNumber,
                website = p.Website,
                description = p.ProviderDescription,
                totalAudience = p.TotalAudience,
                // Queried through the DbSet rather than p.ProviderChannel: the scaffold
                // reads the partial unique index as one-to-one, which is wrong.
                primaryChannelId = _adsConnectContext.ProviderChannels
                    .Where(c => c.ProviderId == p.ProviderId && c.IsPrimary)
                    .Select(c => (Guid?)c.ChannelId)
                    .FirstOrDefault(),
                primaryChannel = _adsConnectContext.ProviderChannels
                    .Where(c => c.ProviderId == p.ProviderId && c.IsPrimary)
                    .Select(c => c.Channel.ChannelName)
                    .FirstOrDefault(),
            });
    }
}
