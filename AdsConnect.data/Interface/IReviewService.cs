using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetReviewsService(Guid? userId, string? role);
        Task<(bool created, string message, ReviewDto? review)> CreateReviewService(Guid advertiserUserId, CreateReviewDto dto);
        Task<(bool replied, string message)> ReplyToReviewService(Guid providerUserId, ReplyToReviewDto dto);
        Task<(bool updated, string message)> TogglePublishService(Guid adminUserId, Guid reviewId);
    }
}
