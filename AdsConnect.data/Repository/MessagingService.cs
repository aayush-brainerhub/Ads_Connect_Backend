using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    /// <summary>
    /// Threads and messages. Conversations are created by RequestService when an
    /// advertiser contacts a provider, so there is no "new conversation" endpoint —
    /// you get a thread by sending a request.
    /// </summary>
    public class MessagingService : IMessagingService
    {
        private readonly AdsConnectContext _adsConnectContext;

        public MessagingService(AdsConnectContext adsConnectContext)
        {
            _adsConnectContext = adsConnectContext;
        }

        public async Task<List<ConversationDto>> GetConversationsService(Guid userId)
        {
            try
            {
                return await _adsConnectContext.ConversationParticipants
                    .Where(p => p.UserId == userId && p.IsActive)
                    .OrderByDescending(p => p.Conversation.LastMessageDate ?? p.Conversation.CreatedDate)
                    .Select(p => new ConversationDto
                    {
                        id = p.ConversationId,
                        // Named after whoever else is in the thread. Falls back to the
                        // subject for a thread with no other active participant.
                        name = _adsConnectContext.ConversationParticipants
                            .Where(o => o.ConversationId == p.ConversationId && o.UserId != userId && o.IsActive)
                            .Select(o => o.User.LastName == null
                                ? o.User.FirstName
                                : o.User.FirstName + " " + o.User.LastName)
                            .FirstOrDefault() ?? (p.Conversation.Subject ?? "Conversation"),
                        subject = p.Conversation.Subject,
                        lastMessage = _adsConnectContext.Messages
                            .Where(m => m.ConversationId == p.ConversationId && m.DeletedDate == null)
                            .OrderByDescending(m => m.MessageSeq)
                            .Select(m => m.MessageText)
                            .FirstOrDefault(),
                        lastMessageDate = p.Conversation.LastMessageDate == null
                            ? null
                            : p.Conversation.LastMessageDate.Value.ToString("o"),
                        // LastReadSeq is a watermark over the global Message identity
                        // sequence, so "unread" is simply everything past it that
                        // somebody else sent.
                        unread = _adsConnectContext.Messages
                            .Count(m => m.ConversationId == p.ConversationId
                                     && m.MessageSeq > p.LastReadSeq
                                     && m.SenderId != userId
                                     && m.DeletedDate == null),
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ChatMessageDto>?> GetMessagesService(Guid userId, Guid conversationId)
        {
            try
            {
                var participant = await _adsConnectContext.ConversationParticipants
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId && p.IsActive);

                if (participant == null)
                {
                    return null;
                }

                var messages = await _adsConnectContext.Messages
                    .Where(m => m.ConversationId == conversationId && m.DeletedDate == null)
                    .OrderBy(m => m.MessageSeq)
                    .Select(m => new ChatMessageDto
                    {
                        id = m.MessageId,
                        conversationId = m.ConversationId,
                        fromMe = m.SenderId == userId,
                        text = m.MessageText,
                        sentDate = m.SentDate.ToString("o"),
                        senderName = m.Sender == null
                            ? null
                            : (m.Sender.LastName == null
                                ? m.Sender.FirstName
                                : m.Sender.FirstName + " " + m.Sender.LastName),
                    })
                    .ToListAsync();

                // Opening the thread is what marks it read, which is why this is not a
                // pure read. The watermark only ever moves forward.
                var newest = await _adsConnectContext.Messages
                    .Where(m => m.ConversationId == conversationId)
                    .MaxAsync(m => (long?)m.MessageSeq) ?? 0;

                if (newest > participant.LastReadSeq)
                {
                    participant.LastReadSeq = newest;
                    participant.LastReadDate = DateTime.UtcNow;
                    await _adsConnectContext.SaveChangesAsync();
                }

                return messages;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool sent, string message, ChatMessageDto? created)> SendMessageService(
            Guid userId, SendMessageDto dto)
        {
            try
            {
                var text = (dto.text ?? string.Empty).Trim();

                // CK_Message_Body rejects an empty Text message at the database level.
                if (text.Length == 0)
                {
                    return (false, "A message cannot be empty.", null);
                }

                var conversation = await _adsConnectContext.Conversations
                    .Where(c => c.ConversationId == dto.conversationId)
                    .Select(c => new
                    {
                        c.ConversationId,
                        c.IsLocked,
                        isParticipant = c.ConversationParticipants.Any(p => p.UserId == userId && p.IsActive),
                    })
                    .FirstOrDefaultAsync();

                if (conversation == null || !conversation.isParticipant)
                {
                    return (false, "Conversation not found.", null);
                }

                if (conversation.IsLocked)
                {
                    return (false, "This conversation is closed.", null);
                }

                var message = new Message
                {
                    MessageId = Guid.NewGuid(),
                    ConversationId = dto.conversationId,
                    SenderId = userId,
                    MessageType = "Text",
                    MessageText = text,
                    Payload = "{}",
                };

                // MessageSeq is an identity-always column and Conversation.MessageCount
                // and LastMessageDate are maintained by the BumpConversationActivity
                // trigger, so neither is set here.
                _adsConnectContext.Messages.Add(message);
                await _adsConnectContext.SaveChangesAsync();

                return (true, "Message sent", new ChatMessageDto
                {
                    id = message.MessageId,
                    conversationId = message.ConversationId,
                    fromMe = true,
                    text = message.MessageText,
                    sentDate = message.SentDate.ToString("o"),
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
