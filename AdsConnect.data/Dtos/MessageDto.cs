namespace AdsConnect.data.Dtos
{
    public class ConversationDto
    {
        public Guid id { get; set; }

        public string name { get; set; } = string.Empty;

        public string? subject { get; set; }
        public string? lastMessage { get; set; }

        public string? lastMessageDate { get; set; }

        public int unread { get; set; }
    }

    public class ChatMessageDto
    {
        public Guid id { get; set; }
        public Guid conversationId { get; set; }
        public bool fromMe { get; set; }

        public string? text { get; set; }
        public string sentDate { get; set; } = string.Empty;
        public string? senderName { get; set; }
    }

    public class SendMessageDto
    {
        public Guid conversationId { get; set; }
        public string text { get; set; } = string.Empty;
    }
}
