using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace AdsConnect.data.Model;

public partial class ProviderPricing
{
    public Guid PricingId { get; set; }

    public Guid InventoryId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid PricingUnitId { get; set; }

    public string? PricingName { get; set; }

    public decimal UnitPrice { get; set; }

    public string Currency { get; set; } = null!;

    public decimal? MinimumQuantity { get; set; }

    public decimal? MaximumQuantity { get; set; }

    public string PriceTiers { get; set; } = null!;

    public NpgsqlRange<DateOnly> ValidPeriod { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual PricingUnit PricingUnit { get; set; } = null!;

    public virtual ProviderInventory ProviderInventory { get; set; } = null!;
}
