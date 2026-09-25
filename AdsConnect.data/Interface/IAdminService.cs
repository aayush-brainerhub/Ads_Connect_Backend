using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IAdminService
    {
        Task<AdminMetricsDto> GetMetricsService();

        Task<List<AdminUserDto>> GetUsersService(string? search);

        Task<bool> ApproveProviderService(Guid userId);
    }
}
