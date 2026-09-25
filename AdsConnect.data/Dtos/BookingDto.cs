using System.ComponentModel.DataAnnotations;

namespace AdsConnect.data.Dtos
{
    public class ProposalItemDto
    {
        public Guid id { get; set; }
        public Guid proposalId { get; set; }
        public Guid? inventoryId { get; set; }
        public string channelName { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int quantity { get; set; } = 1;
        public decimal unitPrice { get; set; }
        public decimal totalPrice { get; set; }
    }

    public class ProposalDto
    {
        public Guid id { get; set; }
        public Guid requestId { get; set; }
        public string campaignTitle { get; set; } = string.Empty;
        public string advertiserName { get; set; } = string.Empty;
        public Guid providerId { get; set; }
        public string providerName { get; set; } = string.Empty;
        public short version { get; set; } = 1;
        public string description { get; set; } = string.Empty;
        public string deliverablesSummary { get; set; } = string.Empty;
        public string termsConditions { get; set; } = string.Empty;
        public decimal subtotal { get; set; }
        public decimal discountAmount { get; set; }
        public decimal taxAmount { get; set; }
        public decimal totalAmount { get; set; }
        public string currency { get; set; } = "USD";
        public string startDate { get; set; } = string.Empty;
        public string endDate { get; set; } = string.Empty;
        public string validUntil { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string submittedDate { get; set; } = string.Empty;
        public string? decidedDate { get; set; }
        public string? rejectionReason { get; set; }
        public List<ProposalItemDto> items { get; set; } = new();
    }

    public class CreateProposalDto
    {
        [Required]
        public Guid requestId { get; set; }

        public string description { get; set; } = string.Empty;
        public string deliverablesSummary { get; set; } = string.Empty;
        public string termsConditions { get; set; } = string.Empty;

        [Range(0, 100000000)]
        public decimal totalAmount { get; set; }

        public string? startDate { get; set; }
        public string? endDate { get; set; }
        public string? validUntil { get; set; }

        public List<ProposalItemDto> items { get; set; } = new();
    }

    public class RespondToProposalDto
    {
        [Required]
        public Guid proposalId { get; set; }

        [Required]
        public bool accept { get; set; }

        public string? rejectionReason { get; set; }
    }

    public class BookingItemDto
    {
        public Guid id { get; set; }
        public Guid bookingId { get; set; }
        public string channelName { get; set; } = string.Empty;
        public string itemTitle { get; set; } = string.Empty;
        public string scheduledDate { get; set; } = string.Empty;
        public decimal unitPrice { get; set; }
        public int quantity { get; set; } = 1;
        public decimal totalPrice { get; set; }
        public string status { get; set; } = string.Empty;
    }

    public class CampaignDeliverableDto
    {
        public Guid id { get; set; }
        public Guid bookingId { get; set; }
        public string bookingNumber { get; set; } = string.Empty;
        public string campaignTitle { get; set; } = string.Empty;
        public string deliverableType { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string dueDate { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string? submittedDate { get; set; }
        public string? proofUrl { get; set; }
        public string? proofMetrics { get; set; }
        public string? reviewComments { get; set; }
        public string? reviewedDate { get; set; }
        public short revisionCount { get; set; }
    }

    public class SubmitDeliverableProofDto
    {
        [Required]
        public Guid deliverableId { get; set; }

        [Required]
        public string proofUrl { get; set; } = string.Empty;

        public string? proofMetrics { get; set; }
    }

    public class ReviewDeliverableDto
    {
        [Required]
        public Guid deliverableId { get; set; }

        [Required]
        public bool approved { get; set; }

        public string? comments { get; set; }
    }

    public class BookingDto
    {
        public Guid id { get; set; }
        public string bookingNumber { get; set; } = string.Empty;
        public Guid proposalId { get; set; }
        public Guid campaignId { get; set; }
        public string campaignTitle { get; set; } = string.Empty;
        public Guid advertiserId { get; set; }
        public string advertiserName { get; set; } = string.Empty;
        public Guid providerId { get; set; }
        public string providerName { get; set; } = string.Empty;
        public decimal totalAmount { get; set; }
        public decimal platformFeeAmount { get; set; }
        public decimal taxAmount { get; set; }
        public decimal providerPayout { get; set; }
        public string currency { get; set; } = "USD";
        public string startDate { get; set; } = string.Empty;
        public string endDate { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string bookedDate { get; set; } = string.Empty;
        public string? confirmedDate { get; set; }
        public string? completedDate { get; set; }
        public List<BookingItemDto> items { get; set; } = new();
        public List<CampaignDeliverableDto> deliverables { get; set; } = new();
    }
}
