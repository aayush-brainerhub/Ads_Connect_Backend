using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class PricingUnit
{
    public Guid PricingUnitId { get; set; }

    public string UnitCode { get; set; } = null!;

    public string UnitName { get; set; } = null!;

    public bool IsTimeBased { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<ProposalItem> ProposalItems { get; set; } = new List<ProposalItem>();

    public virtual ICollection<ProviderPricing> ProviderPricings { get; set; } = new List<ProviderPricing>();
}
