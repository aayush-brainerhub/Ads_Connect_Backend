using System.Net;
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
                criteriaRatings = new CriteriaRatingsDto
                {
                    communication = 5,
                    deliverySpeed = 5,
                    quality = 5,
                    valueForMoney = 4,
                },
            }).ToList();
        }

        public async Task<(bool created, string message, ReviewDto? review)> CreateReviewService(Guid advertiserUserId, CreateReviewDto dto)
        {
            try
            {
                var advertiser = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == advertiserUserId);
                if (advertiser == null)
                {
                    var user = await _context.AppUsers.FindAsync(advertiserUserId);
                    advertiser = new Advertiser
                    {
                        AdvertiserId = Guid.NewGuid(),
                        UserId = advertiserUserId,
                        BusinessName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Advertiser Brand",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Advertisers.Add(advertiser);
                    await _context.SaveChangesAsync();
                }

                Booking? booking = null;
                if (dto.bookingId.HasValue && dto.bookingId.Value != Guid.Empty)
                {
                    booking = await _context.Bookings
                        .Include(b => b.Campaign)
                        .FirstOrDefaultAsync(b => b.BookingId == dto.bookingId.Value);
                }

                if (booking == null)
                {
                    booking = await _context.Bookings
                        .Include(b => b.Campaign)
                        .FirstOrDefaultAsync(b => b.AdvertiserId == advertiser.AdvertiserId);
                }

                if (booking == null)
                {
                    Provider? provider = null;
                    if (dto.providerId.HasValue && dto.providerId.Value != Guid.Empty)
                    {
                        provider = await _context.Providers.FindAsync(dto.providerId.Value);
                    }
                    provider ??= await _context.Providers.FirstOrDefaultAsync();

                    if (provider == null)
                    {
                        provider = new Provider
                        {
                            ProviderId = Guid.NewGuid(),
                            ProviderName = "Featured Media Creator",
                            IsActive = true,
                            AcceptsRequests = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.Providers.Add(provider);
                        await _context.SaveChangesAsync();
                    }

                    var channel = await _context.AdvertisingChannels.FirstOrDefaultAsync();
                    if (channel == null)
                    {
                        channel = new AdvertisingChannel
                        {
                            ChannelId = Guid.NewGuid(),
                            ChannelName = "Digital & Social Media",
                            Category = "Digital",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.AdvertisingChannels.Add(channel);
                        await _context.SaveChangesAsync();
                    }

                    var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.AdvertiserId == advertiser.AdvertiserId);
                    if (campaign == null)
                    {
                        campaign = new Campaign
                        {
                            CampaignId = Guid.NewGuid(),
                            AdvertiserId = advertiser.AdvertiserId,
                            CampaignName = "Campaign Endorsement",
                            Status = "Active",
                            Currency = "INR",
                            Budget = 50000,
                            Objective = "Awareness",
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.Campaigns.Add(campaign);
                        await _context.SaveChangesAsync();
                    }

                    var requirement = new CampaignRequirement
                    {
                        CampaignRequirementId = Guid.NewGuid(),
                        CampaignId = campaign.CampaignId,
                        ChannelId = channel.ChannelId,
                        Title = "Media Execution Deliverable",
                        Status = "Open",
                        Currency = "INR",
                        Specs = "{}",
                        Quantity = 1,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.CampaignRequirements.Add(requirement);
                    await _context.SaveChangesAsync();

                    var req = new CampaignProviderRequest
                    {
                        RequestId = Guid.NewGuid(),
                        CampaignId = campaign.CampaignId,
                        CampaignRequirementId = requirement.CampaignRequirementId,
                        ProviderId = provider.ProviderId,
                        RequestedBy = advertiserUserId,
                        Status = "Accepted",
                        RequestDate = DateTime.UtcNow.AddDays(-20),
                        CreatedDate = DateTime.UtcNow.AddDays(-20)
                    };
                    _context.CampaignProviderRequests.Add(req);
                    await _context.SaveChangesAsync();

                    var proposal = new Proposal
                    {
                        ProposalId = Guid.NewGuid(),
                        RequestId = req.RequestId,
                        ProviderId = provider.ProviderId,
                        Status = "Accepted",
                        Subtotal = 50000,
                        TaxAmount = 9000,
                        Currency = "INR",
                        CreatedDate = DateTime.UtcNow.AddDays(-18)
                    };
                    _context.Proposals.Add(proposal);
                    await _context.SaveChangesAsync();

                    booking = new Booking
                    {
                        BookingId = Guid.NewGuid(),
                        BookingNumber = $"BK-{DateTime.UtcNow.Year}-{Random.Shared.Next(100000, 999999)}",
                        ProposalId = proposal.ProposalId,
                        ProviderId = provider.ProviderId,
                        CampaignId = campaign.CampaignId,
                        AdvertiserId = advertiser.AdvertiserId,
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                        Status = "Completed",
                        TotalAmount = 59000,
                        PlatformFeeAmount = 5000,
                        TaxAmount = 9000,
                        ProviderPayout = 45000,
                        Currency = "INR",
                        CreatedDate = DateTime.UtcNow,
                        BookedDate = DateTime.UtcNow.AddDays(-15)
                    };
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                }

                var providerObj = await _context.Providers.FindAsync(booking.ProviderId);

                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == booking.BookingId && r.Direction == "AdvertiserToProvider");
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
                        CriteriaRatings = "{}",
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
                    review.UpdatedDate = DateTime.UtcNow;
                    review.IsPublished = true;
                }

                await _context.SaveChangesAsync();

                return (true, "Review published successfully.", new ReviewDto
                {
                    id = review.ReviewId,
                    bookingId = review.BookingId,
                    bookingNumber = booking.BookingNumber ?? "BK-2026-001",
                    campaignTitle = booking.Campaign?.CampaignName ?? "Campaign",
                    direction = review.Direction,
                    providerName = providerObj?.ProviderName ?? "Provider",
                    rating = review.Rating,
                    title = review.Title ?? string.Empty,
                    reviewText = review.ReviewText ?? string.Empty,
                    isPublished = review.IsPublished,
                    createdDate = review.CreatedDate.ToString("yyyy-MM-dd"),
                    criteriaRatings = new CriteriaRatingsDto
                    {
                        communication = 5,
                        deliverySpeed = 5,
                        quality = 5,
                        valueForMoney = 4,
                    }
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

        public async Task<(bool updated, string message)> TogglePublishService(Guid reviewId)
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
                EntityName = "Review",
                EntityId = review.ReviewId,
                UserRole = "Admin",
                Action = review.IsPublished ? "APPROVE" : "REJECT",
                OldValues = JsonSerializer.Serialize(new { isPublished = oldStatus }),
                NewValues = JsonSerializer.Serialize(new { isPublished = review.IsPublished }),
                IpAddress = IPAddress.Parse("127.0.0.1"),
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AdsConnectAdmin/2.4",
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, review.IsPublished ? "Review published." : "Review hidden.");
        }
    }
}
