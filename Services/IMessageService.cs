using Eryth.Models;
using Eryth.ViewModels;

namespace Eryth.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageViewModel>> GetUserMessagesAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadMessageCountAsync(Guid userId);
        Task<MessageViewModel?> GetMessageByIdAsync(Guid messageId, Guid currentUserId);
        Task<MessageViewModel> SendMessageAsync(CreateMessageViewModel messageViewModel, Guid senderId);
        Task<bool> MarkAsReadAsync(Guid messageId, Guid currentUserId);
        Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserId);
        Task<IEnumerable<MessageViewModel>> GetConversationAsync(Guid userId, Guid otherUserId);
        Task<MessageConversationViewModel?> GetConversationByMessageAsync(Guid messageId, Guid currentUserId);
        Task<MessageViewModel?> SendReplyAsync(Guid conversationId, string content, Guid senderId);
        Task<IEnumerable<MessageViewModel>> GetLatestMessagesAsync(Guid userId, int count = 5);
        Task<IEnumerable<UserSearchViewModel>> SearchUsersAsync(string query, Guid currentUserId, int maxResults = 10);
    }
}
