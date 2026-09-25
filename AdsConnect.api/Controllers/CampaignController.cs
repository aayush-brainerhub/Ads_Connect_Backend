using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampaignController : ApiControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpGet("GetMyCampaigns")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> GetMyCampaigns() =>
            ForCurrentUser<List<CampaignDto>>(async userId =>
                Success(await _campaignService.GetMyCampaignsService(userId), "my campaigns"));

        [HttpPost("CreateCampaign")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> CreateCampaign(CreateCampaignDto dto) =>
            ForCurrentUser<CampaignDto>(async userId =>
            {
                var (created, message, campaign) = await _campaignService.CreateCampaignService(userId, dto);
                return created ? Success(campaign!, message) : Failure<CampaignDto>(message);
            });

        [HttpPut("UpdateCampaign")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> UpdateCampaign(UpdateCampaignDto dto) =>
            ForCurrentUser<CampaignDto>(async userId =>
            {
                var (updated, message, campaign) = await _campaignService.UpdateCampaignService(userId, dto);
                return updated ? Success(campaign!, message) : Failure<CampaignDto>(message);
            });

        [HttpDelete("DeleteCampaign")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> DeleteCampaign(Guid id) =>
            ForCurrentUser<string>(async userId =>
            {
                var (deleted, message) = await _campaignService.DeleteCampaignService(userId, id);
                return deleted ? Success(message, message) : Failure<string>(message);
            });

        [HttpGet("GetAllCampaigns")]
        [Authorize(Roles = "Admin")]
        public Task<ActionResult> GetAllCampaigns() =>
            ForCurrentUser<List<CampaignDto>>(async _ =>
                Success(await _campaignService.GetAllCampaignsService(), "all campaigns"));
    }
}
