using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Booking
{
    public Guid BookingId { get; set; }

    public string BookingNumber { get; set; } = null!;

    public Guid ProposalId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid AdvertiserId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PlatformFeeAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ProviderPayout { get; set; }

    public string Currency { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime BookedDate { get; set; }

    public DateTime? ConfirmedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual ICollection<CampaignDeliverable> CampaignDeliverables { get; set; } = new List<CampaignDeliverable>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<InventoryAvailability> InventoryAvailabilities { get; set; } = new List<InventoryAvailability>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<PlatformFee> PlatformFees { get; set; } = new List<PlatformFee>();

    public virtual Proposal Proposal { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
