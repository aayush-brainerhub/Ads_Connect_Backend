using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class CampaignDeliverable
{
    public Guid DeliverableId { get; set; }

    public Guid BookingId { get; set; }

    public Guid? BookingItemId { get; set; }

    public string DeliverableType { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateOnly? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SubmittedDate { get; set; }

    public Guid? SubmittedBy { get; set; }

    public string? ProofUrl { get; set; }

    public string Proof { get; set; } = null!;

    public string Metrics { get; set; } = null!;

    public Guid? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public string? Comments { get; set; }

    public short RevisionCount { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual BookingItem? BookingItem { get; set; }

    public virtual AppUser? ReviewedByNavigation { get; set; }

    public virtual AppUser? SubmittedByNavigation { get; set; }
}
