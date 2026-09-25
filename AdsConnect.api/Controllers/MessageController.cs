using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ApiControllerBase
    {
        private readonly IMessagingService _messagingService;

        public MessageController(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        [HttpGet("GetConversations")]
        public Task<ActionResult> GetConversations() =>
            ForCurrentUser<List<ConversationDto>>(async userId =>
                Success(await _messagingService.GetConversationsService(userId), "conversations"));

        [HttpGet("GetMessages")]
        public Task<ActionResult> GetMessages(Guid conversationId) =>
            ForCurrentUser<List<ChatMessageDto>>(async userId =>
            {
                var messages = await _messagingService.GetMessagesService(userId, conversationId);
               
                return messages == null
                    ? Failure<List<ChatMessageDto>>("Conversation not found.")
                    : Success(messages, "messages");
            });

        [HttpPost("SendMessage")]
        public Task<ActionResult> SendMessage(SendMessageDto dto) =>
            ForCurrentUser<ChatMessageDto>(async userId =>
            {
                var (sent, message, created) = await _messagingService.SendMessageService(userId, dto);
                return sent ? Success(created!, message) : Failure<ChatMessageDto>(message);
            });
    }
}
