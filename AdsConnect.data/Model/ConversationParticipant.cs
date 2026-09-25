using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class ConversationParticipant
{
    public Guid ConversationParticipantId { get; set; }

    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }

    public string ParticipantRole { get; set; } = null!;

    public DateTime JoinedDate { get; set; }

    public DateTime? LeftDate { get; set; }

    public long LastReadSeq { get; set; }

    public DateTime? LastReadDate { get; set; }

    public bool IsMuted { get; set; }

    public bool IsActive { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
