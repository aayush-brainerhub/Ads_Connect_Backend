using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class ProviderLocation
{
    public Guid ProviderLocationId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid LocationId { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? PostalCode { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int? ServiceRadiusKm { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Provider Provider { get; set; } = null!;

    public virtual ICollection<ProviderInventory> ProviderInventories { get; set; } = new List<ProviderInventory>();
}
