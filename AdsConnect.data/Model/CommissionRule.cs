using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class CommissionRule
{
    public Guid CommissionRuleId { get; set; }

    public string Scope { get; set; } = null!;

    public Guid? ProviderTypeId { get; set; }

    public Guid? ChannelId { get; set; }

    public Guid? ProviderId { get; set; }

    public Guid? AdvertiserId { get; set; }

    public decimal PercentageRate { get; set; }

    public decimal FixedAmount { get; set; }

    public decimal? MinFee { get; set; }

    public decimal? MaxFee { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public short Priority { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Advertiser? Advertiser { get; set; }

    public virtual AdvertisingChannel? Channel { get; set; }

    public virtual AppUser? CreatedByNavigation { get; set; }

    public virtual ICollection<PlatformFee> PlatformFees { get; set; } = new List<PlatformFee>();

    public virtual Provider? Provider { get; set; }

    public virtual ProviderType? ProviderType { get; set; }
}
