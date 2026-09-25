using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace AdsConnect.data.Model;

public partial class InventoryAvailability
{
    public Guid AvailabilityId { get; set; }

    public Guid InventoryId { get; set; }

    public Guid? BookingId { get; set; }

    public NpgsqlRange<DateOnly> Period { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ProviderInventory Inventory { get; set; } = null!;
}
