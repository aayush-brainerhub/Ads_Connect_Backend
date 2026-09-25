using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ApiControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("GetProposals")]
        public Task<ActionResult> GetProposals() =>
            ForCurrentUser<List<ProposalDto>>(async userId =>
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
                return Success(await _bookingService.GetProposalsService(userId, role), "proposals");
            });

        [HttpPost("CreateProposal")]
        [Authorize(Roles = "Provider")]
        public Task<ActionResult> CreateProposal(CreateProposalDto dto) =>
            ForCurrentUser<ProposalDto>(async userId =>
            {
                var (created, message, proposal) = await _bookingService.CreateProposalService(userId, dto);
                return created ? Success(proposal!, message) : Failure<ProposalDto>(message);
            });

        [HttpPost("RespondToProposal")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> RespondToProposal(RespondToProposalDto dto) =>
            ForCurrentUser<BookingDto>(async userId =>
            {
                var (success, message, booking) = await _bookingService.RespondToProposalService(userId, dto);
                return success ? Success(booking!, message) : Failure<BookingDto>(message);
            });

        [HttpGet("GetBookings")]
        public Task<ActionResult> GetBookings() =>
            ForCurrentUser<List<BookingDto>>(async userId =>
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
                return Success(await _bookingService.GetBookingsService(userId, role), "bookings");
            });

        [HttpGet("GetDeliverables")]
        public Task<ActionResult> GetDeliverables() =>
            ForCurrentUser<List<CampaignDeliverableDto>>(async userId =>
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
                return Success(await _bookingService.GetDeliverablesService(userId, role), "deliverables");
            });

        [HttpPost("SubmitDeliverableProof")]
        [Authorize(Roles = "Provider")]
        public Task<ActionResult> SubmitDeliverableProof(SubmitDeliverableProofDto dto) =>
            ForCurrentUser<string>(async userId =>
            {
                var (success, message) = await _bookingService.SubmitDeliverableProofService(userId, dto);
                return success ? Success(message, message) : Failure<string>(message);
            });

        [HttpPost("ReviewDeliverable")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> ReviewDeliverable(ReviewDeliverableDto dto) =>
            ForCurrentUser<string>(async userId =>
            {
                var (success, message) = await _bookingService.ReviewDeliverableService(userId, dto);
                return success ? Success(message, message) : Failure<string>(message);
            });
    }
}
