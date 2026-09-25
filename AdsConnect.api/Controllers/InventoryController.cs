using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Provider")]
    public class InventoryController : ApiControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("GetMyInventory")]
        public Task<ActionResult> GetMyInventory() =>
            ForCurrentUser<List<InventoryDto>>(async userId =>
                Success(await _inventoryService.GetMyInventoryService(userId), "my inventory"));

        [HttpPost("SaveInventory")]
        public Task<ActionResult> SaveInventory(SaveInventoryDto dto) =>
            ForCurrentUser<InventoryDto>(async userId =>
            {
                var (saved, message, item) = await _inventoryService.SaveInventoryService(userId, dto);
                return saved ? Success(item!, message) : Failure<InventoryDto>(message);
            });

        [HttpDelete("DeleteInventory")]
        public Task<ActionResult> DeleteInventory(Guid id) =>
            ForCurrentUser<string>(async userId =>
            {
                var (deleted, message) = await _inventoryService.DeleteInventoryService(userId, id);
                return deleted ? Success(message, message) : Failure<string>(message);
            });
    }
}
