using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class BookingItem
{
    public Guid BookingItemId { get; set; }

    public Guid BookingId { get; set; }

    public Guid? InventoryId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? PricingUnitCode { get; set; }

    public string ItemSpecs { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public short SortOrder { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<CampaignDeliverable> CampaignDeliverables { get; set; } = new List<CampaignDeliverable>();

    public virtual ProviderInventory? Inventory { get; set; }
}
