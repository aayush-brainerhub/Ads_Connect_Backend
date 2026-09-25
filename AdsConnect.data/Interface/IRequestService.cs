using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IRequestService
    {
        Task<(bool created, string message, Guid? conversationId)> CreateRequestService(
            Guid userId, CreateRequestDto dto);

        Task<List<ProviderRequestDto>> GetProviderRequestsService(Guid userId);

        Task<List<ProviderRequestDto>> GetAdvertiserRequestsService(Guid userId);

        Task<(bool updated, string message)> RespondToRequestService(Guid userId, RespondToRequestDto dto);
    }
}
