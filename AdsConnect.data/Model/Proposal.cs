using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Proposal
{
    public Guid ProposalId { get; set; }

    public Guid RequestId { get; set; }

    public Guid ProviderId { get; set; }

    public short Version { get; set; }

    public string? Description { get; set; }

    public string? Deliverables { get; set; }

    public string? TermsConditions { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public string Currency { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime? ValidUntil { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SubmittedDate { get; set; }

    public DateTime? DecidedDate { get; set; }

    public Guid? DecidedBy { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual CampaignProviderRequest CampaignProviderRequest { get; set; } = null!;

    public virtual AppUser? DecidedByNavigation { get; set; }

    public virtual ICollection<ProposalItem> ProposalItems { get; set; } = new List<ProposalItem>();
}
