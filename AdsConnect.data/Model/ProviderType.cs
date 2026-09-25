using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class ProviderType
{
    public Guid ProviderTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public short SortOrder { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CampaignRequirement> CampaignRequirements { get; set; } = new List<CampaignRequirement>();

    public virtual ICollection<CommissionRule> CommissionRules { get; set; } = new List<CommissionRule>();

    public virtual ICollection<Provider> Providers { get; set; } = new List<Provider>();
}
