using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class PlatformFee
{
    public Guid PlatformFeeId { get; set; }

    public Guid BookingId { get; set; }

    public Guid? CommissionRuleId { get; set; }

    public string FeeType { get; set; } = null!;

    public string RuleSnapshot { get; set; } = null!;

    public decimal BaseAmount { get; set; }

    public decimal? Percentage { get; set; }

    public decimal? FixedAmount { get; set; }

    public decimal CalculatedAmount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime CalculatedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual CommissionRule? CommissionRule { get; set; }
}
