using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class CampaignRequirement
{
    public Guid CampaignRequirementId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid? ProviderTypeId { get; set; }

    public string Title { get; set; } = null!;

    public string? Requirements { get; set; }

    public decimal Quantity { get; set; }

    public decimal? BudgetMin { get; set; }

    public decimal? BudgetMax { get; set; }

    public string Currency { get; set; } = null!;

    public long? MinAudience { get; set; }

    public decimal? MaxUnitPrice { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Specs { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int RequestsCount { get; set; }

    public int ProposalsCount { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual ICollection<CampaignProviderRequest> CampaignProviderRequests { get; set; } = new List<CampaignProviderRequest>();

    public virtual AdvertisingChannel Channel { get; set; } = null!;

    public virtual ProviderType? ProviderType { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
