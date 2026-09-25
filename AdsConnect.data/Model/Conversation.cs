using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Conversation
{
    public Guid ConversationId { get; set; }

    public string ContextType { get; set; } = null!;

    public Guid? CampaignId { get; set; }

    public Guid? RequestId { get; set; }

    public Guid? BookingId { get; set; }

    public string? Subject { get; set; }

    public bool IsAdminMediated { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LastMessageDate { get; set; }

    public int MessageCount { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Campaign? Campaign { get; set; }

    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual CampaignProviderRequest? Request { get; set; }
}
