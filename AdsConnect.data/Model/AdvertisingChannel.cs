using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class AdvertisingChannel
{
    public Guid ChannelId { get; set; }

    public string ChannelName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public string MetadataSchema { get; set; } = null!;

    public bool IsActive { get; set; }

    public short SortOrder { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CampaignRequirement> CampaignRequirements { get; set; } = new List<CampaignRequirement>();

    public virtual ICollection<CommissionRule> CommissionRules { get; set; } = new List<CommissionRule>();

    public virtual ICollection<ProviderChannel> ProviderChannels { get; set; } = new List<ProviderChannel>();

    public virtual ICollection<ProviderInventory> ProviderInventories { get; set; } = new List<ProviderInventory>();
}
