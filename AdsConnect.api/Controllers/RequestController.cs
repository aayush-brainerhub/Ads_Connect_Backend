using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestController : ApiControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost("CreateRequest")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> CreateRequest(CreateRequestDto dto) =>
            ForCurrentUser<Guid>(async userId =>
            {
                var (created, message, conversationId) = await _requestService.CreateRequestService(userId, dto);
                return created ? Success(conversationId!.Value, message) : Failure<Guid>(message);
            });

        [HttpGet("GetProviderRequests")]
        [Authorize(Roles = "Provider")]
        public Task<ActionResult> GetProviderRequests() =>
            ForCurrentUser<List<ProviderRequestDto>>(async userId =>
                Success(await _requestService.GetProviderRequestsService(userId), "provider requests"));

        [HttpGet("GetMyRequests")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> GetMyRequests() =>
            ForCurrentUser<List<ProviderRequestDto>>(async userId =>
                Success(await _requestService.GetAdvertiserRequestsService(userId), "my requests"));

        [HttpPost("RespondToRequest")]
        [Authorize(Roles = "Provider")]
        public Task<ActionResult> RespondToRequest(RespondToRequestDto dto) =>
            ForCurrentUser<string>(async userId =>
            {
                var (updated, message) = await _requestService.RespondToRequestService(userId, dto);
                return updated ? Success(message, message) : Failure<string>(message);
            });
    }
}
