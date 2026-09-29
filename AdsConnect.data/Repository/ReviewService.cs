using System.Text.Json;
using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class ReviewService : IReviewService
    {
        private readonly AdsConnectContext _context;

        public ReviewService(AdsConnectContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewDto>> GetReviewsService(Guid? userId, string? role)
        {
            var query = _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Campaign)
                .Include(r => r.Provider)
                .Include(r => r.Advertiser)
                .Include(r => r.ReviewerUser)
                .AsQueryable();

            if (role == "Advertiser" && userId.HasValue)
            {
                var adv = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == userId.Value);
                if (adv != null)
                {
                    query = query.Where(r => r.AdvertiserId == adv.AdvertiserId || r.IsPublished);
                }
            }
            else if (role == "Provider" && userId.HasValue)
            {
                var prov = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId.Value);
                if (prov != null)
                {
                    query = query.Where(r => r.ProviderId == prov.ProviderId || r.IsPublished);
                }
            }
            else if (role != "Admin")
            {
                query = query.Where(r => r.IsPublished);
            }

            var reviews = await query
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return reviews.Select(r => new ReviewDto
            {
                id = r.ReviewId,
                bookingId = r.BookingId,
                bookingNumber = r.Booking?.BookingNumber ?? string.Empty,
                campaignTitle = r.Booking?.Campaign?.CampaignName ?? "Campaign",
                direction = r.Direction,
                reviewerName = r.ReviewerUser == null ? "Anonymous" : $"{r.ReviewerUser.FirstName} {r.ReviewerUser.LastName}".Trim(),
                reviewerRole = r.Direction == "AdvertiserToProvider" ? "Advertiser" : "Provider",
                providerId = r.ProviderId,
                providerName = r.Provider?.ProviderName ?? "Provider",
                advertiserId = r.AdvertiserId,
                advertiserName = r.Advertiser?.BusinessName ?? "Advertiser",
                rating = r.Rating,
                title = r.Title ?? string.Empty,
                reviewText = r.ReviewText ?? string.Empty,
                isPublished = r.IsPublished,
                responseText = r.ResponseText,
                responseDate = r.ResponseDate?.ToString("yyyy-MM-dd"),
                createdDate = r.CreatedDate.ToString("yyyy-MM-dd"),
                criteriaRatings = ParseCriteria(r.CriteriaRatings),
            }).ToList();
        }

        public async Task<(bool created, string message, ReviewDto? review)> CreateReviewService(Guid advertiserUserId, CreateReviewDto dto)
        {
            try
            {
                const string NoCompletedBooking = "You can only review a provider after a completed booking.";

                // A review must hang off a real, completed booking between this
                // advertiser and the provider. Nothing is created here to make one up.
                var advertiser = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == advertiserUserId);
                if (advertiser == null)
                {
                    return (false, NoCompletedBooking, null);
                }

                var hasBookingId = dto.bookingId.HasValue && dto.bookingId.Value != Guid.Empty;
                var hasProviderId = dto.providerId.HasValue && dto.providerId.Value != Guid.Empty;

                if (!hasBookingId && !hasProviderId)
                {
                    return (false, "Choose the booking or provider you want to review.", null);
                }

                var bookings = _context.Bookings
                    .Include(b => b.Campaign)
                    .Where(b => b.AdvertiserId == advertiser.AdvertiserId && b.Status == "Completed");

                if (hasBookingId)
                {
                    bookings = bookings.Where(b => b.BookingId == dto.bookingId!.Value);
                }

                if (hasProviderId)
                {
                    bookings = bookings.Where(b => b.ProviderId == dto.providerId!.Value);
                }

                var booking = await bookings
                    .OrderByDescending(b => b.CompletedDate ?? b.CreatedDate)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return (false, NoCompletedBooking, null);
                }

                var providerObj = await _context.Providers.FindAsync(booking.ProviderId);

                var review = await _context.Reviews
                    .AsTracking()
                    .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId && r.Direction == "AdvertiserToProvider");
                if (review == null)
                {
                    review = new Review
                    {
                        ReviewId = Guid.NewGuid(),
                        BookingId = booking.BookingId,
                        Direction = "AdvertiserToProvider",
                        ReviewerUserId = advertiserUserId,
                        ProviderId = booking.ProviderId,
                        AdvertiserId = advertiser.AdvertiserId,
                        Rating = dto.rating,
                        Title = dto.title,
                        ReviewText = dto.reviewText,
                        CriteriaRatings = SerializeCriteria(dto.criteriaRatings),
                        IsPublished = true,
                        CreatedDate = DateTime.UtcNow,
                    };
                    _context.Reviews.Add(review);
                }
                else
                {
                    review.Rating = dto.rating;
                    review.Title = dto.title;
                    review.ReviewText = dto.reviewText;
                    review.CriteriaRatings = SerializeCriteria(dto.criteriaRatings);
                    review.UpdatedDate = DateTime.UtcNow;
                    review.IsPublished = true;
                }

                await _context.SaveChangesAsync();

                return (true, "Review published successfully.", new ReviewDto
                {
                    id = review.ReviewId,
                    bookingId = review.BookingId,
                    bookingNumber = booking.BookingNumber,
                    campaignTitle = booking.Campaign?.CampaignName ?? "Campaign",
                    direction = review.Direction,
                    reviewerRole = "Advertiser",
                    providerId = review.ProviderId,
                    providerName = providerObj?.ProviderName ?? "Provider",
                    advertiserId = review.AdvertiserId,
                    advertiserName = advertiser.BusinessName,
                    rating = review.Rating,
                    title = review.Title ?? string.Empty,
                    reviewText = review.ReviewText ?? string.Empty,
                    isPublished = review.IsPublished,
                    createdDate = review.CreatedDate.ToString("yyyy-MM-dd"),
                    criteriaRatings = ParseCriteria(review.CriteriaRatings),
                });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? $": {ex.InnerException.Message}" : "";
                return (false, $"{ex.Message}{inner}", null);
            }
        }

        public async Task<(bool replied, string message)> ReplyToReviewService(Guid providerUserId, ReplyToReviewDto dto)
        {
            var review = await _context.Reviews.FindAsync(dto.reviewId);
            if (review == null)
            {
                return (false, "Review not found.");
            }

            review.ResponseText = dto.responseText;
            review.ResponseDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Response posted successfully.");
        }

        public async Task<(bool updated, string message)> TogglePublishService(Guid adminUserId, Guid reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return (false, "Review not found.");
            }

            var oldStatus = review.IsPublished;
            review.IsPublished = !review.IsPublished;
            review.UpdatedDate = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                UserId = adminUserId,
                EntityName = "Review",
                EntityId = review.ReviewId,
                UserRole = "Admin",
                Action = review.IsPublished ? "APPROVE" : "REJECT",
                OldValues = JsonSerializer.Serialize(new { isPublished = oldStatus }),
                NewValues = JsonSerializer.Serialize(new { isPublished = review.IsPublished }),
                // No request context reaches this layer, so the caller's IP and user
                // agent are unknown; they are left empty rather than invented.
                IpAddress = null,
                UserAgent = null,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, review.IsPublished ? "Review published." : "Review hidden.");
        }

        private static string SerializeCriteria(CriteriaRatingsDto? criteria) =>
            criteria == null ? "{}" : JsonSerializer.Serialize(criteria);

        /// <summary>
        /// Returns the per-criterion ratings the reviewer actually gave, or null when
        /// none were submitted (stored as "{}"), instead of showing default scores.
        /// </summary>
        private static CriteriaRatingsDto? ParseCriteria(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var criteria = JsonSerializer.Deserialize<CriteriaRatingsDto>(json);
                if (criteria == null) return null;

                var anyGiven = criteria.communication > 0 || criteria.deliverySpeed > 0
                    || criteria.quality > 0 || criteria.valueForMoney > 0;
                return anyGiven ? criteria : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
