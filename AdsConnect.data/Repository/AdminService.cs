using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class AdminService : IAdminService
    {
        private readonly AdsConnectContext _adsConnectContext;

        public AdminService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<AdminMetricsDto> GetMetricsService()
        {
            try
            {
                return new AdminMetricsDto
                {
                    totalProviders = await _adsConnectContext.Providers.CountAsync(p => p.IsActive),
                    totalAdvertisers = await _adsConnectContext.Advertisers.CountAsync(a => a.IsActive),
                    totalCampaigns = await _adsConnectContext.Campaigns.CountAsync(),
                    activeCampaigns = await _adsConnectContext.Campaigns.CountAsync(c => c.Status == "Active"),
                    totalUsers = await _adsConnectContext.AppUsers.CountAsync(u => u.IsActive),
                    // Deliberately not called revenue: Payment and Invoice are empty, so
                    // there is nothing settled to report. This is what advertisers have
                    // committed across live campaigns.
                    committedBudget = await _adsConnectContext.Campaigns
                        .Where(c => c.Status == "Active")
                        .SumAsync(c => (decimal?)c.Budget) ?? 0m,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<AdminUserDto>> GetUsersService(string? search)
        {
            try
            {
                var query = _adsConnectContext.AppUsers.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(u => u.FirstName.ToLower().Contains(term)
                                          || (u.LastName != null && u.LastName.ToLower().Contains(term))
                                          || u.Email.ToLower().Contains(term));
                }

                var users = await query
                    .OrderByDescending(u => u.CreatedDate)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.IsActive,
                        u.CreatedDate,
                    })
                    .ToListAsync();

                var ids = users.Select(u => u.UserId).ToList();

                // Fetched separately and joined in memory: a user may hold several
                // roles, and string.Join has no SQL translation.
                var roles = await _adsConnectContext.UserRoles
                    .Where(r => ids.Contains(r.UserId) && r.Role.IsActive)
                    .OrderBy(r => r.Role.RoleName)
                    .Select(r => new { r.UserId, r.Role.RoleName })
                    .ToListAsync();

                var rolesByUser = roles
                    .GroupBy(r => r.UserId)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(r => r.RoleName)));

                return users.Select(u => new AdminUserDto
                {
                    id = u.UserId,
                    name = u.LastName == null ? u.FirstName : $"{u.FirstName} {u.LastName}",
                    email = u.Email,
                    role = rolesByUser.TryGetValue(u.UserId, out var roleNames) ? roleNames : string.Empty,
                    joined = u.CreatedDate.ToString("yyyy-MM-dd"),
                    status = u.IsActive ? "Active" : "Inactive",
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ApproveProviderService(Guid userId)
        {
            try
            {
                var rows = await _adsConnectContext.AppUsers
                    .Where(u => u.UserId == userId && !u.IsActive)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, true));
                
                return rows > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
