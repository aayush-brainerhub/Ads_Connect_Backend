using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Provider
{
    public Guid ProviderId { get; set; }

    public Guid UserId { get; set; }

    public Guid ProviderTypeId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string? LegalName { get; set; }

    public string? ProviderDescription { get; set; }

    public Guid? PrimaryLocationId { get; set; }

    public string? Website { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? LogoUrl { get; set; }

    public string BaseCurrency { get; set; } = null!;

    public decimal Rating { get; set; }

    public int RatingCount { get; set; }

    public long TotalAudience { get; set; }

    public int CompletedBookings { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }

    public bool AcceptsRequests { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<CampaignProviderRequest> CampaignProviderRequests { get; set; } = new List<CampaignProviderRequest>();

    public virtual ICollection<CommissionRule> CommissionRules { get; set; } = new List<CommissionRule>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Location? PrimaryLocation { get; set; }

    public virtual ProviderChannel? ProviderChannel { get; set; }

    public virtual ICollection<ProviderInventory> ProviderInventories { get; set; } = new List<ProviderInventory>();

    public virtual ProviderLocation? ProviderLocation { get; set; }

    public virtual ProviderType ProviderType { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual AppUser User { get; set; } = null!;
}
