using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IProviderService
    {
        Task<List<ProviderDto>> GetAllProviderService();

        Task<ProviderDto?> GetProviderByIdService(Guid id);
    }
}
