using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface ICampaignService
    {
        Task<List<CampaignDto>> GetMyCampaignsService(Guid userId);

        Task<(bool created, string message, CampaignDto? campaign)> CreateCampaignService(
            Guid userId, CreateCampaignDto dto);

        Task<List<CampaignDto>> GetAllCampaignsService();

        Task<(bool updated, string message, CampaignDto? campaign)> UpdateCampaignService(
            Guid userId, UpdateCampaignDto dto);

        Task<(bool deleted, string message)> DeleteCampaignService(
            Guid userId, Guid id);
    }
}
