using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Campaign
{
    public Guid CampaignId { get; set; }

    public Guid AdvertiserId { get; set; }

    public string CampaignName { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? IndustryId { get; set; }

    public string Objective { get; set; } = null!;

    public string? ObjectiveDescription { get; set; }

    public decimal Budget { get; set; }

    public string Currency { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string TargetAudience { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int RequirementsCount { get; set; }

    public int ResponsesCount { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Advertiser Advertiser { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<CampaignRequirement> CampaignRequirements { get; set; } = new List<CampaignRequirement>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual Industry? Industry { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
