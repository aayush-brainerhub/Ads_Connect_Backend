using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class CampaignProviderRequest
{
    public Guid RequestId { get; set; }

    public Guid CampaignRequirementId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid? InventoryId { get; set; }

    public Guid RequestedBy { get; set; }

    public string? Message { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public DateTime? ViewedDate { get; set; }

    public DateTime? ResponseDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? DeclineReason { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual CampaignRequirement CampaignRequirement { get; set; } = null!;

    public virtual Conversation? Conversation { get; set; }

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual Provider Provider { get; set; } = null!;

    public virtual ProviderInventory? ProviderInventory { get; set; }

    public virtual AppUser RequestedByNavigation { get; set; } = null!;
}
