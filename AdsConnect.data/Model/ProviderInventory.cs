using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class ProviderInventory
{
    public Guid InventoryId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid ChannelId { get; set; }

    public Guid? ProviderLocationId { get; set; }

    public string InventoryName { get; set; } = null!;

    public string? InventoryCode { get; set; }

    public string? Description { get; set; }

    public string Specs { get; set; } = null!;

    public int QuantityTotal { get; set; }

    public short LeadTimeDays { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

    public virtual ICollection<CampaignProviderRequest> CampaignProviderRequests { get; set; } = new List<CampaignProviderRequest>();

    public virtual AdvertisingChannel Channel { get; set; } = null!;

    public virtual ICollection<InventoryAvailability> InventoryAvailabilities { get; set; } = new List<InventoryAvailability>();

    public virtual ICollection<ProposalItem> ProposalItems { get; set; } = new List<ProposalItem>();

    public virtual Provider Provider { get; set; } = null!;

    public virtual ProviderLocation? ProviderLocation { get; set; }

    public virtual ICollection<ProviderPricing> ProviderPricings { get; set; } = new List<ProviderPricing>();
}
