using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class ProviderService : IProviderService
    {
        private readonly AdsConnectContext _adsConnectContext;

        public ProviderService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<List<ProviderDto>> GetAllProviderService()
        {
            try
            {
                // Ordering happens on the entity query, before projection: ordering the
                // projected DTOs instead makes EF try to translate an ORDER BY over the
                // constructed ProviderDto and the query fails to translate at all.
                return await Project(
                        _adsConnectContext.Providers
                            .Where(p => p.IsActive)
                            .OrderBy(p => p.ProviderName))
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ProviderDto?> GetProviderByIdService(Guid id)
        {
            try
            {
                return await Project(
                        _adsConnectContext.Providers
                            .Where(p => p.IsActive && p.ProviderId == id))
                    .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Flattens a provider into the card shape in a single query.
        /// </summary>
        /// <remarks>
        /// Deliberately queries ProviderChannels/ProviderPricings through the DbSets
        /// rather than the scaffolded navigations. ProviderChannel and ProviderLocation
        /// carry PARTIAL unique indexes (UX_*_Primary, "... WHERE IsPrimary"); EF Core
        /// cannot see the filter, reads them as plain unique indexes on ProviderId and
        /// therefore scaffolds Provider.ProviderChannel / .ProviderLocation as
        /// one-to-one references. That is wrong - a provider may own many channels and
        /// many locations, only one of each being primary - and the generated
        /// navigations would break as soon as a second row exists. Correlated
        /// sub-queries sidestep the bad model and survive re-scaffolding.
        /// </remarks>
        private IQueryable<ProviderDto> Project(IQueryable<Provider> source) =>
            source.Select(p => new ProviderDto
            {
                id = p.ProviderId,
                name = p.ProviderName,
                type = p.ProviderType.Name,
                location = p.PrimaryLocation == null ? null : p.PrimaryLocation.City,
                category = _adsConnectContext.ProviderChannels
                    .Where(pc => pc.ProviderId == p.ProviderId && pc.IsPrimary)
                    .Select(pc => pc.Channel.Category)
                    .FirstOrDefault(),
                // A provider with no active pricing yet reads as 0 rather than failing.
                price = _adsConnectContext.ProviderPricings
                    .Where(pp => pp.ProviderId == p.ProviderId && pp.IsActive)
                    .Select(pp => (decimal?)pp.UnitPrice)
                    .Min() ?? 0m,
                audience = p.TotalAudience,
                rating = p.Rating,
                description = p.ProviderDescription,
            });
    }
}
