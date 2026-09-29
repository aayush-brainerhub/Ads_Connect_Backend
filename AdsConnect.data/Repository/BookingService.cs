using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class BookingService : IBookingService
    {
        private readonly AdsConnectContext _context;

        public BookingService(AdsConnectContext context)
        {
            _context = context;
        }

        public async Task<List<ProposalDto>> GetProposalsService(Guid userId, string role)
        {
            var query = _context.Proposals
                .Include(p => p.CampaignProviderRequest)
                    .ThenInclude(r => r.CampaignRequirement)
                        .ThenInclude(cr => cr.Campaign)
                            .ThenInclude(c => c.Advertiser)
                .Include(p => p.ProposalItems)
                    .ThenInclude(i => i.Inventory)
                        .ThenInclude(inv => inv!.Channel)
                .AsQueryable();

            if (role == "Provider")
            {
                var prov = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
                if (prov == null) return new List<ProposalDto>();
                query = query.Where(p => p.ProviderId == prov.ProviderId);
            }
            else if (role == "Advertiser")
            {
                var adv = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == userId);
                if (adv == null) return new List<ProposalDto>();
                query = query.Where(p => p.CampaignProviderRequest.CampaignRequirement.Campaign.AdvertiserId == adv.AdvertiserId);
            }

            var proposals = await query
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            var providerIds = proposals.Select(p => p.ProviderId).Distinct().ToList();
            var providerNames = await _context.Providers
                .Where(p => providerIds.Contains(p.ProviderId))
                .ToDictionaryAsync(p => p.ProviderId, p => p.ProviderName);

            return proposals.Select(p => new ProposalDto
            {
                id = p.ProposalId,
                requestId = p.RequestId,
                campaignTitle = p.CampaignProviderRequest?.CampaignRequirement?.Campaign?.CampaignName ?? "Campaign Request",
                advertiserName = p.CampaignProviderRequest?.CampaignRequirement?.Campaign?.Advertiser?.BusinessName ?? "Advertiser",
                providerId = p.ProviderId,
                providerName = providerNames.GetValueOrDefault(p.ProviderId) ?? string.Empty,
                version = p.Version,
                description = p.Description ?? string.Empty,
                deliverablesSummary = p.Deliverables ?? string.Empty,
                termsConditions = p.TermsConditions ?? string.Empty,
                subtotal = p.Subtotal,
                discountAmount = p.DiscountAmount,
                taxAmount = p.TaxAmount,
                totalAmount = p.TotalAmount ?? (p.Subtotal + p.TaxAmount - p.DiscountAmount),
                currency = p.Currency,
                startDate = p.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                endDate = p.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                validUntil = p.ValidUntil?.ToString("yyyy-MM-dd") ?? string.Empty,
                status = p.Status,
                submittedDate = p.SubmittedDate?.ToString("yyyy-MM-dd") ?? p.CreatedDate.ToString("yyyy-MM-dd"),
                decidedDate = p.DecidedDate?.ToString("yyyy-MM-dd"),
                rejectionReason = p.RejectionReason,
                items = p.ProposalItems.Select(i => new ProposalItemDto
                {
                    id = i.ProposalItemId,
                    proposalId = i.ProposalId,
                    inventoryId = i.InventoryId,
                    channelName = i.Inventory?.Channel?.ChannelName ?? string.Empty,
                    description = i.Description ?? string.Empty,
                    quantity = (int)i.Quantity,
                    unitPrice = i.UnitPrice,
                    totalPrice = i.TotalPrice ?? (i.Quantity * i.UnitPrice),
                }).ToList(),
            }).ToList();
        }

        public async Task<(bool created, string message, ProposalDto? proposal)> CreateProposalService(Guid providerUserId, CreateProposalDto dto)
        {
            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == providerUserId);
            if (provider == null)
            {
                return (false, "Provider profile not found.", null);
            }

            var request = await _context.CampaignProviderRequests
                .Include(r => r.CampaignRequirement)
                    .ThenInclude(cr => cr.Campaign)
                        .ThenInclude(c => c.Advertiser)
                .FirstOrDefaultAsync(r => r.RequestId == dto.requestId);

            if (request == null)
            {
                return (false, "Campaign request not found.", null);
            }

            var proposal = new Proposal
            {
                ProposalId = Guid.NewGuid(),
                RequestId = dto.requestId,
                ProviderId = provider.ProviderId,
                Version = 1,
                Description = dto.description,
                Deliverables = dto.deliverablesSummary,
                TermsConditions = dto.termsConditions,
                Subtotal = dto.totalAmount,
                DiscountAmount = 0,
                TaxAmount = dto.totalAmount * 0.1m,
                TotalAmount = dto.totalAmount * 1.1m,
                Currency = "USD",
                Status = "Submitted",
                SubmittedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
            };

            if (DateOnly.TryParse(dto.startDate, out var sDate)) proposal.StartDate = sDate;
            if (DateOnly.TryParse(dto.endDate, out var eDate)) proposal.EndDate = eDate;
            if (DateTime.TryParse(dto.validUntil, out var vDate)) proposal.ValidUntil = vDate;

            _context.Proposals.Add(proposal);

            if (dto.items != null && dto.items.Count > 0)
            {
                foreach (var item in dto.items)
                {
                    _context.ProposalItems.Add(new ProposalItem
                    {
                        ProposalItemId = Guid.NewGuid(),
                        ProposalId = proposal.ProposalId,
                        InventoryId = item.inventoryId,
                        Description = item.description,
                        Quantity = item.quantity,
                        UnitPrice = item.unitPrice,
                        TotalPrice = item.totalPrice,
                        ItemSpecs = "{}",
                    });
                }
            }

            await _context.SaveChangesAsync();

            return (true, "Proposal sent successfully.", new ProposalDto
            {
                id = proposal.ProposalId,
                requestId = proposal.RequestId,
                campaignTitle = request.CampaignRequirement?.Campaign?.CampaignName ?? "Campaign",
                advertiserName = request.CampaignRequirement?.Campaign?.Advertiser?.BusinessName ?? "Advertiser",
                providerId = provider.ProviderId,
                providerName = provider.ProviderName,
                totalAmount = proposal.TotalAmount ?? proposal.Subtotal,
                currency = proposal.Currency,
                status = proposal.Status,
                submittedDate = proposal.SubmittedDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
            });
        }

        public async Task<(bool success, string message, BookingDto? booking)> RespondToProposalService(Guid advertiserUserId, RespondToProposalDto dto)
        {
            var advertiser = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == advertiserUserId);
            if (advertiser == null)
            {
                return (false, "Advertiser profile not found.", null);
            }

            var proposal = await _context.Proposals
                .Include(p => p.CampaignProviderRequest)
                    .ThenInclude(r => r.CampaignRequirement)
                .Include(p => p.ProposalItems)
                .FirstOrDefaultAsync(p => p.ProposalId == dto.proposalId);

            if (proposal == null)
            {
                return (false, "Proposal not found.", null);
            }

            proposal.DecidedDate = DateTime.UtcNow;
            proposal.DecidedBy = advertiserUserId;

            if (!dto.accept)
            {
                proposal.Status = "Rejected";
                proposal.RejectionReason = dto.rejectionReason ?? "Declined by advertiser.";
                await _context.SaveChangesAsync();
                return (true, "Proposal declined.", null);
            }

            proposal.Status = "Accepted";

            // Generate Booking
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                BookingNumber = $"BKG-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                ProposalId = proposal.ProposalId,
                ProviderId = proposal.ProviderId,
                CampaignId = proposal.CampaignProviderRequest.CampaignId,
                AdvertiserId = advertiser.AdvertiserId,
                TotalAmount = proposal.TotalAmount ?? proposal.Subtotal,
                PlatformFeeAmount = (proposal.TotalAmount ?? proposal.Subtotal) * 0.12m,
                TaxAmount = proposal.TaxAmount,
                ProviderPayout = (proposal.TotalAmount ?? proposal.Subtotal) * 0.88m,
                Currency = proposal.Currency,
                StartDate = proposal.StartDate,
                EndDate = proposal.EndDate,
                Status = "Confirmed",
                BookedDate = DateTime.UtcNow,
                ConfirmedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
            };

            _context.Bookings.Add(booking);

            // Generate initial deliverable task
            var deliverable = new CampaignDeliverable
            {
                DeliverableId = Guid.NewGuid(),
                BookingId = booking.BookingId,
                DeliverableType = "Post",
                Title = $"{proposal.Deliverables ?? "Campaign Deliverables"}",
                Description = proposal.Description,
                DueDate = proposal.EndDate,
                Status = "Pending",
                RevisionCount = 0,
                Proof = "{}",
                Metrics = "{}",
                CreatedDate = DateTime.UtcNow,
            };

            _context.CampaignDeliverables.Add(deliverable);

            // Generate Escrow Invoice
            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                BookingId = booking.BookingId,
                Direction = "AdvertiserToPlatform",
                AdvertiserId = advertiser.AdvertiserId,
                ProviderId = proposal.ProviderId,
                Subtotal = proposal.Subtotal,
                TaxAmount = proposal.TaxAmount,
                TotalAmount = proposal.TotalAmount,
                AmountPaid = 0,
                Currency = proposal.Currency,
                Status = "Issued",
                IssuedDate = DateTime.UtcNow,
                DueDate = proposal.StartDate,
                BillingSnapshot = "{}",
                CreatedDate = DateTime.UtcNow,
            };

            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return (true, "Proposal accepted and booking created.", new BookingDto
            {
                id = booking.BookingId,
                bookingNumber = booking.BookingNumber,
                proposalId = booking.ProposalId,
                campaignId = booking.CampaignId,
                advertiserId = booking.AdvertiserId,
                providerId = booking.ProviderId,
                totalAmount = booking.TotalAmount,
                currency = booking.Currency,
                status = booking.Status,
                bookedDate = booking.BookedDate.ToString("yyyy-MM-dd"),
            });
        }

        public async Task<List<BookingDto>> GetBookingsService(Guid userId, string role)
        {
            var query = _context.Bookings
                .Include(b => b.Campaign)
                .Include(b => b.BookingItems)
                .Include(b => b.CampaignDeliverables)
                .AsQueryable();

            if (role == "Advertiser")
            {
                var adv = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == userId);
                if (adv == null) return new List<BookingDto>();
                query = query.Where(b => b.AdvertiserId == adv.AdvertiserId);
            }
            else if (role == "Provider")
            {
                var prov = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
                if (prov == null) return new List<BookingDto>();
                query = query.Where(b => b.ProviderId == prov.ProviderId);
            }

            var bookings = await query
                .OrderByDescending(b => b.BookedDate)
                .ToListAsync();

            var advertiserIds = bookings.Select(b => b.AdvertiserId).Distinct().ToList();
            var advertiserNames = await _context.Advertisers
                .Where(a => advertiserIds.Contains(a.AdvertiserId))
                .ToDictionaryAsync(a => a.AdvertiserId, a => a.BusinessName);

            var bookingProviderIds = bookings.Select(b => b.ProviderId).Distinct().ToList();
            var providerNames = await _context.Providers
                .Where(p => bookingProviderIds.Contains(p.ProviderId))
                .ToDictionaryAsync(p => p.ProviderId, p => p.ProviderName);

            return bookings.Select(b => new BookingDto
            {
                id = b.BookingId,
                bookingNumber = b.BookingNumber,
                proposalId = b.ProposalId,
                campaignId = b.CampaignId,
                campaignTitle = b.Campaign?.CampaignName ?? "Campaign",
                advertiserId = b.AdvertiserId,
                advertiserName = advertiserNames.GetValueOrDefault(b.AdvertiserId) ?? string.Empty,
                providerId = b.ProviderId,
                providerName = providerNames.GetValueOrDefault(b.ProviderId) ?? string.Empty,
                totalAmount = b.TotalAmount,
                platformFeeAmount = b.PlatformFeeAmount,
                taxAmount = b.TaxAmount,
                providerPayout = b.ProviderPayout,
                currency = b.Currency,
                startDate = b.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                endDate = b.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                status = b.Status,
                bookedDate = b.BookedDate.ToString("yyyy-MM-dd"),
                confirmedDate = b.ConfirmedDate?.ToString("yyyy-MM-dd"),
                completedDate = b.CompletedDate?.ToString("yyyy-MM-dd"),
                items = b.BookingItems.Select(i => new BookingItemDto
                {
                    id = i.BookingItemId,
                    bookingId = i.BookingId,
                    itemTitle = i.Description,
                    scheduledDate = i.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    unitPrice = i.UnitPrice,
                    quantity = (int)i.Quantity,
                    totalPrice = i.TotalPrice ?? (i.Quantity * i.UnitPrice),
                    // BookingItem has no status column; nothing is reported rather than a made-up one.
                    status = string.Empty,
                }).ToList(),
                deliverables = b.CampaignDeliverables.Select(d => new CampaignDeliverableDto
                {
                    id = d.DeliverableId,
                    bookingId = d.BookingId,
                    bookingNumber = b.BookingNumber,
                    campaignTitle = b.Campaign?.CampaignName ?? "Campaign",
                    deliverableType = d.DeliverableType,
                    title = d.Title ?? d.DeliverableType,
                    description = d.Description ?? string.Empty,
                    dueDate = d.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    status = d.Status,
                    submittedDate = d.SubmittedDate?.ToString("yyyy-MM-dd HH:mm"),
                    proofUrl = d.ProofUrl,
                    reviewComments = d.Comments,
                    revisionCount = d.RevisionCount,
                }).ToList(),
            }).ToList();
        }

        public async Task<List<CampaignDeliverableDto>> GetDeliverablesService(Guid userId, string role)
        {
            var query = _context.CampaignDeliverables
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Campaign)
                .AsQueryable();

            if (role == "Advertiser")
            {
                var adv = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == userId);
                if (adv == null) return new List<CampaignDeliverableDto>();
                query = query.Where(d => d.Booking.AdvertiserId == adv.AdvertiserId);
            }
            else if (role == "Provider")
            {
                var prov = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
                if (prov == null) return new List<CampaignDeliverableDto>();
                query = query.Where(d => d.Booking.ProviderId == prov.ProviderId);
            }

            var deliverables = await query
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            return deliverables.Select(d => new CampaignDeliverableDto
            {
                id = d.DeliverableId,
                bookingId = d.BookingId,
                bookingNumber = d.Booking?.BookingNumber ?? string.Empty,
                campaignTitle = d.Booking?.Campaign?.CampaignName ?? "Campaign",
                deliverableType = d.DeliverableType,
                title = d.Title ?? d.DeliverableType,
                description = d.Description ?? string.Empty,
                dueDate = d.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                status = d.Status,
                submittedDate = d.SubmittedDate?.ToString("yyyy-MM-dd HH:mm"),
                proofUrl = d.ProofUrl,
                reviewComments = d.Comments,
                reviewedDate = d.ReviewedDate?.ToString("yyyy-MM-dd"),
                revisionCount = d.RevisionCount,
            }).ToList();
        }

        public async Task<(bool success, string message)> SubmitDeliverableProofService(Guid providerUserId, SubmitDeliverableProofDto dto)
        {
            var deliverable = await _context.CampaignDeliverables.FindAsync(dto.deliverableId);
            if (deliverable == null)
            {
                return (false, "Deliverable not found.");
            }

            deliverable.ProofUrl = dto.proofUrl;
            deliverable.Status = "Submitted";
            deliverable.SubmittedDate = DateTime.UtcNow;
            deliverable.SubmittedBy = providerUserId;

            await _context.SaveChangesAsync();
            return (true, "Proof submitted successfully for advertiser review.");
        }

        public async Task<(bool success, string message)> ReviewDeliverableService(Guid advertiserUserId, ReviewDeliverableDto dto)
        {
            var deliverable = await _context.CampaignDeliverables
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.DeliverableId == dto.deliverableId);

            if (deliverable == null)
            {
                return (false, "Deliverable not found.");
            }

            deliverable.ReviewedBy = advertiserUserId;
            deliverable.ReviewedDate = DateTime.UtcNow;
            deliverable.Comments = dto.comments;

            if (dto.approved)
            {
                deliverable.Status = "Approved";
            }
            else
            {
                deliverable.Status = "RevisionRequested";
                deliverable.RevisionCount = (short)(deliverable.RevisionCount + 1);
            }

            await _context.SaveChangesAsync();
            return (true, dto.approved ? "Deliverable approved." : "Revision requested.");
        }
    }
}
