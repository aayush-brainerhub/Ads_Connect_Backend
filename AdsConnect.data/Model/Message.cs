using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Message
{
    public Guid MessageId { get; set; }

    public long MessageSeq { get; set; }

    public Guid ConversationId { get; set; }

    public Guid? SenderId { get; set; }

    public string MessageType { get; set; } = null!;

    public string? MessageText { get; set; }

    public string? AttachmentUrl { get; set; }

    public string Payload { get; set; } = null!;

    public DateTime SentDate { get; set; }

    public DateTime? EditedDate { get; set; }

    public DateTime? DeletedDate { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual AppUser? Sender { get; set; }
}
