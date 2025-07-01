using Microsoft.EntityFrameworkCore;
using Eryth.Data;
using Eryth.Models;
using Eryth.ViewModels;
using Eryth.Models.Enums;

namespace Eryth.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public MessageService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }        public async Task<IEnumerable<MessageViewModel>> GetUserMessagesAsync(Guid userId, int page, int pageSize)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Her kullanıcı için sadece en son mesajı al
            var groupedMessages = messages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.First())
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    SenderAvatarUrl = m.Sender.ProfileImageUrl,
                    ReceiverId = m.ReceiverId,
                    ReceiverUsername = m.Receiver.Username,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    IsRead = m.IsRead,
                    IsDeleted = m.IsDeleted
                });

            return groupedMessages;
        }

        public async Task<int> GetUnreadMessageCountAsync(Guid userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<MessageViewModel?> GetMessageByIdAsync(Guid messageId, Guid currentUserId)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.Id == messageId && 
                           (m.SenderId == currentUserId || m.ReceiverId == currentUserId) && 
                           !m.IsDeleted)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    SenderAvatarUrl = m.Sender.ProfileImageUrl,
                    ReceiverId = m.ReceiverId,
                    ReceiverUsername = m.Receiver.Username,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    IsRead = m.IsRead,
                    IsDeleted = m.IsDeleted
                })
                .FirstOrDefaultAsync();

            return message;
        }        public async Task<MessageViewModel> SendMessageAsync(CreateMessageViewModel messageViewModel, Guid senderId)
        {
            // First, find the receiver by username if ReceiverId is not provided
            if (messageViewModel.ReceiverId == Guid.Empty && !string.IsNullOrEmpty(messageViewModel.RecipientUsername))
            {
                var receiver = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == messageViewModel.RecipientUsername);
                
                if (receiver == null)
                {
                    throw new ArgumentException("Recipient user not found");
                }
                
                messageViewModel.ReceiverId = receiver.Id;
            }

            var message = new Message
            {
                Subject = messageViewModel.Subject.Trim(),
                Content = messageViewModel.Content.Trim(),
                SenderId = senderId,
                ReceiverId = messageViewModel.ReceiverId,
                SentAt = DateTime.UtcNow
            };            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Create notification for receiver
            try
            {
                var sender = await _context.Users.FindAsync(senderId);
                if (sender != null)
                {                    await _notificationService.CreateNotificationAsync(
                        userId: messageViewModel.ReceiverId,
                        type: NotificationType.Message,
                        message: $"New message from {sender.DisplayName ?? sender.Username}: {messageViewModel.Subject}",
                        triggeredByUserId: senderId,
                        relatedMessageId: message.Id,
                        actionUrl: $"/Message/Conversation/{message.Id}"
                    );
                }
            }
            catch (Exception)
            {
                // Notification creation failed, but message was sent successfully
                // Don't fail the entire operation
            }

            // Load the message with navigation properties
            var savedMessage = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstAsync(m => m.Id == message.Id);

            return new MessageViewModel
            {
                Id = savedMessage.Id,
                Subject = savedMessage.Subject,
                Content = savedMessage.Content,
                SenderId = savedMessage.SenderId,
                SenderUsername = savedMessage.Sender.Username,
                SenderAvatarUrl = savedMessage.Sender.ProfileImageUrl,
                ReceiverId = savedMessage.ReceiverId,
                ReceiverUsername = savedMessage.Receiver.Username,
                SentAt = savedMessage.SentAt,
                ReadAt = savedMessage.ReadAt,
                IsRead = savedMessage.IsRead,
                IsDeleted = savedMessage.IsDeleted
            };
        }

        public async Task<bool> MarkAsReadAsync(Guid messageId, Guid currentUserId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && 
                                         m.ReceiverId == currentUserId && 
                                         !m.IsDeleted);

            if (message == null || message.IsRead)
                return false;

            message.MarkAsRead();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && 
                                         (m.SenderId == currentUserId || m.ReceiverId == currentUserId) && 
                                         !m.IsDeleted);

            if (message == null)
                return false;

            message.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MessageViewModel>> GetConversationAsync(Guid userId, Guid otherUserId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId)) &&
                           !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    SenderAvatarUrl = m.Sender.ProfileImageUrl,
                    ReceiverId = m.ReceiverId,
                    ReceiverUsername = m.Receiver.Username,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    IsRead = m.IsRead,
                    IsDeleted = m.IsDeleted
                })
                .ToListAsync();

            return messages;
        }

        public async Task<IEnumerable<MessageViewModel>> GetLatestMessagesAsync(Guid userId, int count = 5)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    SenderAvatarUrl = m.Sender.ProfileImageUrl,
                    ReceiverId = m.ReceiverId,
                    ReceiverUsername = m.Receiver.Username,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    IsRead = m.IsRead,
                    IsDeleted = m.IsDeleted
                })
                .ToListAsync();

            return messages;
        }        public async Task<IEnumerable<UserSearchViewModel>> SearchUsersAsync(string query, Guid currentUserId, int maxResults = 10)
        {
            var users = await _context.Users
                .Where(u => u.Id != currentUserId && 
                           (u.Username.Contains(query) || u.DisplayName.Contains(query)))
                .OrderBy(u => u.Username)
                .Take(maxResults)
                .Select(u => new UserSearchViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    ProfileImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();

            return users;
        }

        public async Task<MessageConversationViewModel?> GetConversationByMessageAsync(Guid messageId, Guid currentUserId)
        {
            // First, get the message to find the other participant
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == messageId && 
                                   (m.SenderId == currentUserId || m.ReceiverId == currentUserId) &&
                                   !m.IsDeleted);

            if (message == null)
                return null;

            // Determine the other user
            var otherUserId = message.SenderId == currentUserId ? message.ReceiverId : message.SenderId;
            var otherUser = message.SenderId == currentUserId ? message.Receiver : message.Sender;

            // Get all messages between these two users
            var conversationMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => ((m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == currentUserId)) &&
                           !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender.Username,
                    SenderAvatarUrl = m.Sender.ProfileImageUrl,
                    ReceiverId = m.ReceiverId,
                    ReceiverUsername = m.Receiver.Username,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt,
                    IsRead = m.IsRead,
                    IsDeleted = m.IsDeleted
                })
                .ToListAsync();            return new MessageConversationViewModel
            {
                Id = messageId,
                Subject = message.Subject,
                CurrentUserId = currentUserId,
                OtherUserId = otherUserId,
                OtherUsername = otherUser.Username,
                OtherDisplayName = otherUser.DisplayName ?? otherUser.Username,
                OtherUserAvatarUrl = otherUser.ProfileImageUrl,
                Messages = conversationMessages,
                LastMessageAt = conversationMessages.LastOrDefault()?.SentAt ?? message.SentAt,
                CanReply = true
            };
        }

        public async Task<MessageViewModel?> SendReplyAsync(Guid conversationId, string content, Guid senderId)
        {
            // Get the original message to find the recipient
            var originalMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == conversationId && !m.IsDeleted);

            if (originalMessage == null)
                return null;

            // Determine the recipient (the other person in the conversation)
            var receiverId = originalMessage.SenderId == senderId ? originalMessage.ReceiverId : originalMessage.SenderId;            // Create reply message
            var reply = new Message
            {
                Id = Guid.NewGuid(),
                Subject = originalMessage.Subject.StartsWith("Re: ") ? originalMessage.Subject : $"Re: {originalMessage.Subject}",
                Content = content,
                SenderId = senderId,
                ReceiverId = receiverId,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                IsDeleted = false
            };

            _context.Messages.Add(reply);
            await _context.SaveChangesAsync();            // Create notification for the recipient
            await _notificationService.CreateNotificationAsync(
                receiverId,
                NotificationType.Message,
                $"You received a reply to: {originalMessage.Subject}",
                senderId,
                relatedMessageId: reply.Id,
                actionUrl: $"/Message/Conversation/{reply.Id}"
            );

            // Get sender info for return
            var sender = await _context.Users.FindAsync(senderId);

            return new MessageViewModel
            {
                Id = reply.Id,
                Subject = reply.Subject,
                Content = reply.Content,
                SenderId = reply.SenderId,
                SenderUsername = sender?.Username ?? "Unknown",
                SenderAvatarUrl = sender?.ProfileImageUrl,
                ReceiverId = reply.ReceiverId,
                ReceiverUsername = "Unknown", // Will be filled by caller if needed
                SentAt = reply.SentAt,
                ReadAt = reply.ReadAt,
                IsRead = reply.IsRead,
                IsDeleted = reply.IsDeleted
            };
        }
    }
}
