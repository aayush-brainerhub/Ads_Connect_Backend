using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IReferenceService
    {
        Task<ReferenceDataDto> GetReferenceDataService();
        Task<LookupsBundleDto> GetLookupsBundleService();
        Task<(bool success, string message)> CreateLookupItemService(CreateLookupItemDto dto);
        Task<(bool success, string message)> ToggleLookupStatusService(ToggleLookupStatusDto dto);
    }
}
