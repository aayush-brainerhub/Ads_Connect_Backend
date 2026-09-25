using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class AppUser
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public bool IsPhoneVerified { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public string Preferences { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Advertiser? Advertiser { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CampaignDeliverable> CampaignDeliverableReviewedByNavigations { get; set; } = new List<CampaignDeliverable>();

    public virtual ICollection<CampaignDeliverable> CampaignDeliverableSubmittedByNavigations { get; set; } = new List<CampaignDeliverable>();

    public virtual ICollection<CampaignProviderRequest> CampaignProviderRequests { get; set; } = new List<CampaignProviderRequest>();

    public virtual ICollection<CommissionRule> CommissionRules { get; set; } = new List<CommissionRule>();

    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Payment> PaymentPayeeUsers { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentPayerUsers { get; set; } = new List<Payment>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual Provider? Provider { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<UserRole> UserRoleAssignedByNavigations { get; set; } = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoleUsers { get; set; } = new List<UserRole>();
}
