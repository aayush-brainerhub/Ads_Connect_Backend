using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ApiControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("GetMetrics")]
        public Task<ActionResult> GetMetrics() =>
            ForCurrentUser<AdminMetricsDto>(async _ =>
                Success(await _adminService.GetMetricsService(), "platform metrics"));

        [HttpGet("GetUsers")]
        public Task<ActionResult> GetUsers(string? search) =>
            ForCurrentUser<List<AdminUserDto>>(async _ =>
                Success(await _adminService.GetUsersService(search), "users"));

        [HttpPut("ApproveProvider/{userId}")]
        public Task<ActionResult> ApproveProvider(Guid userId) =>
            ForCurrentUser<bool>(async _ =>
            {
                var success = await _adminService.ApproveProviderService(userId);
                return success ? Success(true, "Provider approved successfully") : Failure<bool>("Failed to approve provider");
            });
    }
}
