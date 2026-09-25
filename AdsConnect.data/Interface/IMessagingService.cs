using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IMessagingService
    {
        Task<List<ConversationDto>> GetConversationsService(Guid userId);

        Task<List<ChatMessageDto>?> GetMessagesService(Guid userId, Guid conversationId);

        Task<(bool sent, string message, ChatMessageDto? created)> SendMessageService(
            Guid userId, SendMessageDto dto);
    }
}
