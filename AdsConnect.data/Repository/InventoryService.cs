using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class InventoryService : IInventoryService
    {
        private static readonly string[] AllowedStatuses = { "Draft", "Active", "Paused", "Archived" };

        private readonly AdsConnectContext _adsConnectContext;

        public InventoryService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<List<InventoryDto>> GetMyInventoryService(Guid userId)
        {
            try
            {
                return await Project(
                        _adsConnectContext.ProviderInventories
                            .Where(i => i.Provider.UserId == userId && i.Status != "Archived")
                            .OrderBy(i => i.InventoryName))
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool saved, string message, InventoryDto? item)> SaveInventoryService(
            Guid userId, SaveInventoryDto dto)
        {
            try
            {
                var name = (dto.name ?? string.Empty).Trim();
                if (name.Length == 0)
                {
                    return (false, "A name is required.", null);
                }

                if (dto.price < 0)
                {
                    return (false, "The price cannot be negative.", null);
                }

                if (!AllowedStatuses.Contains(dto.status))
                {
                    return (false, $"'{dto.status}' is not a valid inventory status.", null);
                }

                var provider = await _adsConnectContext.Providers
                    .Where(p => p.UserId == userId)
                    .Select(p => new { p.ProviderId, p.BaseCurrency })
                    .FirstOrDefaultAsync();

                if (provider == null)
                {
                    return (false, "Finish provider onboarding before adding inventory.", null);
                }

                if (!await _adsConnectContext.AdvertisingChannels.AnyAsync(c => c.ChannelId == dto.channelId && c.IsActive))
                {
                    return (false, "Choose an advertising channel.", null);
                }

                if (!await _adsConnectContext.PricingUnits.AnyAsync(u => u.PricingUnitId == dto.pricingUnitId && u.IsActive))
                {
                    return (false, "Choose how the price is quoted.", null);
                }

                ProviderInventory inventory;

                if (dto.id == null)
                {
                    inventory = new ProviderInventory
                    {
                        InventoryId = Guid.NewGuid(),
                        ProviderId = provider.ProviderId,
                        Specs = "{}",
                        QuantityTotal = 1,
                    };
                    _adsConnectContext.ProviderInventories.Add(inventory);
                }
                else
                {
                    // Scoped to the caller's own provider, so an id belonging to someone
                    // else reads as "not found" rather than editing their inventory.
                    var existing = await _adsConnectContext.ProviderInventories
                        .AsTracking()
                        .FirstOrDefaultAsync(i => i.InventoryId == dto.id && i.ProviderId == provider.ProviderId);

                    if (existing == null)
                    {
                        return (false, "Inventory not found.", null);
                    }

                    inventory = existing;
                }

                inventory.InventoryName = name;
                inventory.Description = string.IsNullOrWhiteSpace(dto.description) ? null : dto.description.Trim();
                inventory.ChannelId = dto.channelId;
                inventory.Status = dto.status;

                await SaveDefaultPricing(inventory, provider.ProviderId, provider.BaseCurrency, dto);
                await _adsConnectContext.SaveChangesAsync();

                var saved = await Project(
                        _adsConnectContext.ProviderInventories.Where(i => i.InventoryId == inventory.InventoryId))
                    .FirstOrDefaultAsync();

                return (true, "Inventory saved", saved);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool deleted, string message)> DeleteInventoryService(Guid userId, Guid inventoryId)
        {
            try
            {
                var inventory = await _adsConnectContext.ProviderInventories
                    .AsTracking()
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.Provider.UserId == userId);

                if (inventory == null)
                {
                    return (false, "Inventory not found.");
                }

                // Archived rather than removed: BookingItem, ProposalItem and
                // CampaignProviderRequest all point at ProviderInventory, so deleting
                // the row would either fail on the FK or take history with it.
                inventory.Status = "Archived";

                var pricing = await _adsConnectContext.ProviderPricings
                    .AsTracking()
                    .Where(p => p.InventoryId == inventoryId)
                    .ToListAsync();

                foreach (var row in pricing)
                {
                    row.IsActive = false;
                }

                await _adsConnectContext.SaveChangesAsync();

                return (true, "Inventory removed");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <remarks>
        /// EX_Pricing_NoOverlap is a GiST exclusion constraint over
        /// (InventoryId, PricingUnitId, ValidPeriod), so a second row for the same
        /// unit and overlapping dates would be rejected. The existing default row is
        /// updated in place instead of inserting alongside it.
        /// </remarks>
        private async Task SaveDefaultPricing(
            ProviderInventory inventory, Guid providerId, string currency, SaveInventoryDto dto)
        {
            var existing = await _adsConnectContext.ProviderPricings
                .AsTracking()
                .Where(p => p.InventoryId == inventory.InventoryId && p.IsDefault)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                _adsConnectContext.ProviderPricings.Add(new ProviderPricing
                {
                    PricingId = Guid.NewGuid(),
                    InventoryId = inventory.InventoryId,
                    ProviderId = providerId,
                    PricingUnitId = dto.pricingUnitId,
                    PricingName = inventory.InventoryName,
                    UnitPrice = dto.price,
                    Currency = currency,
                    PriceTiers = "[]",
                    IsDefault = true,
                    IsActive = true,
                });
                return;
            }

            existing.PricingUnitId = dto.pricingUnitId;
            existing.PricingName = inventory.InventoryName;
            existing.UnitPrice = dto.price;
            existing.IsActive = true;
        }

        private IQueryable<InventoryDto> Project(IQueryable<ProviderInventory> source) =>
            source.Select(i => new InventoryDto
            {
                id = i.InventoryId,
                name = i.InventoryName,
                description = i.Description,
                channelId = i.ChannelId,
                channel = i.Channel.ChannelName,
                // A slot with no pricing row yet reads as 0 rather than dropping out
                // of the list.
                price = _adsConnectContext.ProviderPricings
                    .Where(p => p.InventoryId == i.InventoryId && p.IsActive)
                    .OrderByDescending(p => p.IsDefault)
                    .Select(p => (decimal?)p.UnitPrice)
                    .FirstOrDefault() ?? 0m,
                pricingUnitId = _adsConnectContext.ProviderPricings
                    .Where(p => p.InventoryId == i.InventoryId && p.IsActive)
                    .OrderByDescending(p => p.IsDefault)
                    .Select(p => (Guid?)p.PricingUnitId)
                    .FirstOrDefault(),
                pricingUnit = _adsConnectContext.ProviderPricings
                    .Where(p => p.InventoryId == i.InventoryId && p.IsActive)
                    .OrderByDescending(p => p.IsDefault)
                    .Select(p => p.PricingUnit.UnitName)
                    .FirstOrDefault(),
                status = i.Status,
            });
    }
}
