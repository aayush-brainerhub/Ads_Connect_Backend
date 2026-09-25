using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IProfileService
    {
        Task<MyProfileDto> GetMyProfileService(Guid userId);

        Task<(bool saved, string message, string? replacedUrl)> SetProfileImageService(Guid userId, string url);

        Task<(bool removed, string message, string? replacedUrl)> RemoveProfileImageService(Guid userId);

        Task<(bool saved, string message, AdvertiserProfileDto? profile)> SaveAdvertiserProfileService(
            Guid userId, SaveAdvertiserProfileDto dto);

        Task<(bool saved, string message, ProviderProfileDto? profile)> SaveProviderProfileService(
            Guid userId, SaveProviderProfileDto dto);
    }
}
