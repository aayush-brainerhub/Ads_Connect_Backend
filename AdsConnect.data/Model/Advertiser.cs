using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Advertiser
{
    public Guid AdvertiserId { get; set; }

    public Guid UserId { get; set; }

    public string BusinessName { get; set; } = null!;

    public string? LegalName { get; set; }

    public string? BusinessDescription { get; set; }

    public Guid? IndustryId { get; set; }

    public Guid? LocationId { get; set; }

    public string? AddressLine { get; set; }

    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public string? Gstnumber { get; set; }

    public decimal? MonthlyBudgetMin { get; set; }

    public decimal? MonthlyBudgetMax { get; set; }

    public string DefaultCurrency { get; set; } = null!;

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual ICollection<CommissionRule> CommissionRules { get; set; } = new List<CommissionRule>();

    public virtual Industry? Industry { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Location? Location { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual AppUser User { get; set; } = null!;
}
