using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class ProviderChannel
{
    public Guid ProviderChannelId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid ChannelId { get; set; }

    public string? Handle { get; set; }

    public string? ProfileUrl { get; set; }

    public long? AudienceSize { get; set; }

    public decimal? EngagementRate { get; set; }

    public string AudienceMetrics { get; set; } = null!;

    public DateTime? VerifiedDate { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual AdvertisingChannel Channel { get; set; } = null!;

    public virtual Provider Provider { get; set; } = null!;
}
