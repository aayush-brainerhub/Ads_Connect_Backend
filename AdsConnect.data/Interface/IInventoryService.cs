using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IInventoryService
    {
        Task<List<InventoryDto>> GetMyInventoryService(Guid userId);

        Task<(bool saved, string message, InventoryDto? item)> SaveInventoryService(
            Guid userId, SaveInventoryDto dto);

        Task<(bool deleted, string message)> DeleteInventoryService(Guid userId, Guid inventoryId);
    }
}
