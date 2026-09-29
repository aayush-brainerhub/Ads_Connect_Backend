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
    public class ReviewController : ApiControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("GetReviews")]
        [AllowAnonymous]
        public async Task<ActionResult> GetReviews()
        {
            var userId = CurrentUserId();
            var role = User.FindFirstValue(ClaimTypes.Role);
            var reviews = await _reviewService.GetReviewsService(userId, role);
            return Ok(Success(reviews, "reviews"));
        }

        [HttpPost("CreateReview")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> CreateReview(CreateReviewDto dto) =>
            ForCurrentUser<ReviewDto>(async userId =>
            {
                var (created, message, review) = await _reviewService.CreateReviewService(userId, dto);
                return created ? Success(review!, message) : Failure<ReviewDto>(message);
            });

        [HttpPost("ReplyToReview")]
        [Authorize(Roles = "Provider")]
        public Task<ActionResult> ReplyToReview(ReplyToReviewDto dto) =>
            ForCurrentUser<string>(async userId =>
            {
                var (replied, message) = await _reviewService.ReplyToReviewService(userId, dto);
                return replied ? Success(message, message) : Failure<string>(message);
            });

        [HttpPut("TogglePublish/{reviewId}")]
        [Authorize(Roles = "Admin")]
        public Task<ActionResult> TogglePublish(Guid reviewId) =>
            ForCurrentUser<string>(async userId =>
            {
                var (updated, message) = await _reviewService.TogglePublishService(userId, reviewId);
                return updated ? Success(message, message) : Failure<string>(message);
            });
    }
}
