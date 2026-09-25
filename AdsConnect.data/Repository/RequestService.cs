using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    /// <summary>
    /// Campaign requests: the advertiser's "Send request" on a provider card, and
    /// the provider's accept / decline inbox.
    ///
    /// A request cannot stand on its own in this schema — CampaignProviderRequest
    /// requires a CampaignRequirement, and the thread the two sides talk in is a
    /// Conversation. Both are created here so the UI's single button does the
    /// whole job.
    /// </summary>
    public class RequestService : IRequestService
    {
        private readonly AdsConnectContext _adsConnectContext;

        public RequestService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<(bool created, string message, Guid? conversationId)> CreateRequestService(
            Guid userId, CreateRequestDto dto)
        {
            try
            {
                var campaign = await _adsConnectContext.Campaigns
                    .Where(c => c.CampaignId == dto.campaignId && c.Advertiser.UserId == userId)
                    .Select(c => new { c.CampaignId, c.CampaignName, c.Budget, c.Currency })
                    .FirstOrDefaultAsync();

                // Also covers a campaign that exists but belongs to someone else: the
                // caller is told the same thing either way.
                if (campaign == null)
                {
                    return (false, "Campaign not found.", null);
                }

                var provider = await _adsConnectContext.Providers
                    .Where(p => p.ProviderId == dto.providerId && p.IsActive)
                    .Select(p => new { p.ProviderId, p.ProviderName, p.UserId, p.AcceptsRequests })
                    .FirstOrDefaultAsync();

                if (provider == null)
                {
                    return (false, "Provider not found.", null);
                }

                if (!provider.AcceptsRequests)
                {
                    return (false, $"{provider.ProviderName} is not accepting requests right now.", null);
                }

                if (provider.UserId == userId)
                {
                    return (false, "You cannot send a request to yourself.", null);
                }

                var duplicate = await _adsConnectContext.CampaignProviderRequests
                    .AnyAsync(r => r.CampaignId == campaign.CampaignId
                                && r.ProviderId == provider.ProviderId
                                && r.Status != "Withdrawn"
                                && r.Status != "Declined");

                if (duplicate)
                {
                    return (false, $"You have already sent {provider.ProviderName} a request for this campaign.", null);
                }

                // CampaignRequirement.ChannelId is NOT NULL, and the channel a provider
                // sells on is their primary ProviderChannel. Queried through the DbSet
                // because the scaffold models that partial unique index as one-to-one.
                var channelId = await _adsConnectContext.ProviderChannels
                    .Where(c => c.ProviderId == provider.ProviderId && c.IsPrimary && c.IsActive)
                    .Select(c => (Guid?)c.ChannelId)
                    .FirstOrDefaultAsync();

                if (channelId == null)
                {
                    return (false, $"{provider.ProviderName} has no advertising channel set up yet.", null);
                }

                var budget = dto.budget > 0 ? dto.budget : campaign.Budget;

                // One requirement per request rather than one shared across providers:
                // the proposed budget differs per provider, and BudgetMin/Max is where
                // this schema keeps it.
                var requirement = new CampaignRequirement
                {
                    CampaignRequirementId = Guid.NewGuid(),
                    CampaignId = campaign.CampaignId,
                    ChannelId = channelId.Value,
                    Title = $"{campaign.CampaignName} — {provider.ProviderName}",
                    Quantity = 1,
                    BudgetMin = budget,
                    BudgetMax = budget,
                    Currency = campaign.Currency,
                    Status = "Open",
                    Specs = "{}",
                };

                var request = new CampaignProviderRequest
                {
                    RequestId = Guid.NewGuid(),
                    CampaignRequirementId = requirement.CampaignRequirementId,
                    CampaignId = campaign.CampaignId,
                    ProviderId = provider.ProviderId,
                    RequestedBy = userId,
                    Message = string.IsNullOrWhiteSpace(dto.message) ? null : dto.message.Trim(),
                    Status = "Pending",
                };

                var conversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    ContextType = "Request",
                    RequestId = request.RequestId,
                    CampaignId = campaign.CampaignId,
                    Subject = campaign.CampaignName,
                };

                _adsConnectContext.CampaignRequirements.Add(requirement);
                _adsConnectContext.CampaignProviderRequests.Add(request);
                _adsConnectContext.Conversations.Add(conversation);

                _adsConnectContext.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationParticipantId = Guid.NewGuid(),
                    ConversationId = conversation.ConversationId,
                    UserId = userId,
                    ParticipantRole = "Advertiser",
                    IsActive = true,
                });

                _adsConnectContext.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationParticipantId = Guid.NewGuid(),
                    ConversationId = conversation.ConversationId,
                    UserId = provider.UserId,
                    ParticipantRole = "Provider",
                    IsActive = true,
                });

                // The covering note becomes the thread's first message, so the provider
                // can simply reply instead of the text living only on the request row.
                if (!string.IsNullOrWhiteSpace(dto.message))
                {
                    _adsConnectContext.Messages.Add(new Message
                    {
                        MessageId = Guid.NewGuid(),
                        ConversationId = conversation.ConversationId,
                        SenderId = userId,
                        MessageType = "Text",
                        MessageText = dto.message.Trim(),
                        Payload = "{}",
                    });
                }

                await _adsConnectContext.SaveChangesAsync();

                return (true, "Request sent", conversation.ConversationId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProviderRequestDto>> GetProviderRequestsService(Guid userId)
        {
            try
            {
                return await Project(
                        _adsConnectContext.CampaignProviderRequests
                            .Where(r => r.Provider.UserId == userId)
                            .OrderByDescending(r => r.RequestDate))
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProviderRequestDto>> GetAdvertiserRequestsService(Guid userId)
        {
            try
            {
                return await Project(
                        _adsConnectContext.CampaignProviderRequests
                            .Where(r => r.RequestedBy == userId)
                            .OrderByDescending(r => r.RequestDate))
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool updated, string message)> RespondToRequestService(Guid userId, RespondToRequestDto dto)
        {
            try
            {
                // CK_Request_Status allows more values than the inbox offers; only the
                // two the buttons produce are accepted here.
                if (dto.status != "Accepted" && dto.status != "Declined")
                {
                    return (false, "A request can only be accepted or declined.");
                }

                var request = await _adsConnectContext.CampaignProviderRequests
                    .AsTracking()
                    .FirstOrDefaultAsync(r => r.RequestId == dto.requestId && r.Provider.UserId == userId);

                if (request == null)
                {
                    return (false, "Request not found.");
                }

                if (request.Status is "Accepted" or "Declined" or "Withdrawn" or "Expired")
                {
                    return (false, $"This request was already {request.Status.ToLowerInvariant()}.");
                }

                request.Status = dto.status;
                request.ResponseDate = DateTime.UtcNow;
                request.DeclineReason = dto.status == "Declined" && !string.IsNullOrWhiteSpace(dto.declineReason)
                    ? dto.declineReason.Trim()
                    : null;

                // Campaign.ResponsesCount and CampaignRequirement.RequestsCount are kept
                // current by the RefreshRequestCounters trigger, so nothing to do here.
                await _adsConnectContext.SaveChangesAsync();

                return (true, $"Request {dto.status.ToLowerInvariant()}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IQueryable<ProviderRequestDto> Project(IQueryable<CampaignProviderRequest> source) =>
            source.Select(r => new ProviderRequestDto
            {
                id = r.RequestId,
                campaignTitle = r.CampaignRequirement.Campaign.CampaignName,
                advertiser = r.CampaignRequirement.Campaign.Advertiser.BusinessName,
                // The per-request budget lives on the requirement this service created
                // for it; the campaign total is the fallback.
                budget = r.CampaignRequirement.BudgetMax ?? r.CampaignRequirement.Campaign.Budget,
                status = r.Status,
                date = r.RequestDate.ToString("yyyy-MM-dd"),
                message = r.Message,
                conversationId = _adsConnectContext.Conversations
                    .Where(c => c.RequestId == r.RequestId)
                    .Select(c => (Guid?)c.ConversationId)
                    .FirstOrDefault(),
            });
    }
}
