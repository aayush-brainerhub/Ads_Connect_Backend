using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IBookingService
    {
        Task<List<ProposalDto>> GetProposalsService(Guid userId, string role);
        Task<(bool created, string message, ProposalDto? proposal)> CreateProposalService(Guid providerUserId, CreateProposalDto dto);
        Task<(bool success, string message, BookingDto? booking)> RespondToProposalService(Guid advertiserUserId, RespondToProposalDto dto);
        Task<List<BookingDto>> GetBookingsService(Guid userId, string role);
        Task<List<CampaignDeliverableDto>> GetDeliverablesService(Guid userId, string role);
        Task<(bool success, string message)> SubmitDeliverableProofService(Guid providerUserId, SubmitDeliverableProofDto dto);
        Task<(bool success, string message)> ReviewDeliverableService(Guid advertiserUserId, ReviewDeliverableDto dto);
    }
}
