using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class ProposalItem
{
    public Guid ProposalItemId { get; set; }

    public Guid ProposalId { get; set; }

    public Guid? InventoryId { get; set; }

    public Guid? PricingUnitId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string ItemSpecs { get; set; } = null!;

    public short SortOrder { get; set; }

    public virtual ProviderInventory? Inventory { get; set; }

    public virtual PricingUnit? PricingUnit { get; set; }

    public virtual Proposal Proposal { get; set; } = null!;
}
