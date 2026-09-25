using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Location
{
    public Guid LocationId { get; set; }

    public string City { get; set; } = null!;

    public string? State { get; set; }

    public string Country { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Advertiser> Advertisers { get; set; } = new List<Advertiser>();

    public virtual ICollection<ProviderLocation> ProviderLocations { get; set; } = new List<ProviderLocation>();

    public virtual ICollection<Provider> Providers { get; set; } = new List<Provider>();

    public virtual ICollection<CampaignRequirement> CampaignRequirements { get; set; } = new List<CampaignRequirement>();

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
