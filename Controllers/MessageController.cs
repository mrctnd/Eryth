using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Eryth.Services;
using Eryth.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Eryth.Controllers
{
    [Authorize]
    public class MessageController : BaseController
    {
        private readonly IMessageService _messageService;
        
        public MessageController(IMessageService messageService, IMemoryCache cache) : base(cache)
        {
            _messageService = messageService;
        }

        // Mesajlar ana sayfa
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var userId = GetRequiredUserId();
                var (validPage, pageSize) = ValidatePagination(page);

                var messages = await _messageService.GetUserMessagesAsync(userId, validPage, pageSize);

                ViewBag.CurrentPage = validPage;
                ViewBag.HasNextPage = messages.Count() == pageSize;

                return View(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception)
            {
                TempData["Error"] = "Messages could not be loaded";
                return View(new List<MessageViewModel>());
            }
        }

        // Navbar için okunmamış mesaj sayısını al
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            if (IsRateLimited("get-unread-messages", 60, TimeSpan.FromMinutes(1)))
            {
                return ErrorResult("Too many requests. Please wait and try again.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var count = await _messageService.GetUnreadMessageCountAsync(userId);

                return SuccessResult(new { unreadCount = count });
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("You need to be logged in");
            }
            catch (Exception)
            {
                return ErrorResult("Could not get message count");
            }
        }

        // Mesajı okundu olarak işaretle
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            if (IsRateLimited("mark-message-read", 100, TimeSpan.FromMinutes(5)))
            {
                return ErrorResult("Too many operations. Please wait 5 minutes.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var result = await _messageService.MarkAsReadAsync(id, userId);
                
                if (result)
                {
                    return SuccessResult(message: "Message marked as read");
                }
                
                return ErrorResult("Message not found or already read");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("You need to be logged in");
            }
            catch (Exception)
            {
                return ErrorResult("Could not update message");
            }
        }

        // Mesajı sil
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (IsRateLimited("delete-message", 20, TimeSpan.FromMinutes(10)))
            {
                return ErrorResult("Too many delete operations. Please wait 10 minutes.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var result = await _messageService.DeleteMessageAsync(id, userId);
                
                if (result)
                {
                    return SuccessResult(message: "Message deleted");
                }
                
                return ErrorResult("Message not found");
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("You need to be logged in");
            }
            catch (Exception)
            {
                return ErrorResult("Could not delete message");
            }
        }

        // Açılır menü için en son mesajları al
        [HttpGet]
        public async Task<IActionResult> GetLatest(int count = 5)
        {
            if (IsRateLimited("get-latest-messages", 30, TimeSpan.FromMinutes(1)))
            {
                return ErrorResult("Too many requests. Please wait and try again.");
            }

            try
            {
                var userId = GetRequiredUserId();
                var messages = await _messageService.GetLatestMessagesAsync(userId, count);

                return SuccessResult(messages.Select(m => new {
                    id = m.Id,
                    subject = m.Subject,
                    senderUsername = m.SenderUsername,
                    senderAvatarUrl = m.SenderAvatarUrl,
                    sentAt = m.RelativeSentTime,
                    isRead = m.IsRead,
                    truncatedContent = m.TruncatedContent
                }));
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("You need to be logged in");
            }
            catch (Exception)
            {
                return ErrorResult("Could not get messages");
            }
        }

        // Mesaj gönder
        [HttpPost]
        public async Task<IActionResult> Send(CreateMessageViewModel model)
        {
            if (IsRateLimited("send-message", 10, TimeSpan.FromMinutes(5)))
            {
                return Json(new { success = false, error = "You are sending messages too frequently. Please wait." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Please fill in all required fields correctly." });
            }

            try
            {
                var userId = GetRequiredUserId();
                var message = await _messageService.SendMessageAsync(model, userId);
                
                return Json(new { success = true, message = "Message sent successfully!" });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, error = "You need to be logged in." });
            }            catch (Exception)
            {
                return Json(new { success = false, error = "An error occurred while sending the message." });
            }
        }

        // Mesaj alıcıları için kullanıcıları ara
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (IsRateLimited("search-users", 100, TimeSpan.FromMinutes(1)))
            {
                return ErrorResult("Too many requests. Please wait and try again.");
            }

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return SuccessResult(new List<object>());
            }

            try
            {
                var userId = GetRequiredUserId();

                // Kullanıcı adı veya görünen ad ile kullanıcıları ara                
                var users = await _messageService.SearchUsersAsync(query, userId, 10);

                return SuccessResult(users.Select(u => new {
                    id = u.Id,
                    username = u.Username,
                    displayName = u.DisplayName,
                    profileImageUrl = u.ProfileImageUrl
                }));
            }
            catch (UnauthorizedAccessException)
            {
                return ErrorResult("You need to be logged in");
            }
            catch (Exception)
            {
                return ErrorResult("Could not search users");
            }
        }

        // Konuşmayı görüntüle
        [HttpGet]
        [Route("Message/Conversation/{messageId:guid}")]
        public async Task<IActionResult> Conversation(Guid messageId)
        {
            Console.WriteLine($"Conversation action called with messageId: {messageId}");
            try
            {
                var userId = GetRequiredUserId();
                Console.WriteLine($"Current userId: {userId}");
                
                Console.WriteLine($"Getting conversation for messageId: {messageId}");
                var conversation = await _messageService.GetConversationByMessageAsync(messageId, userId);
                
                if (conversation == null)
                {
                    Console.WriteLine("Conversation not found!");
                    TempData["Error"] = "Message not found or access denied";
                    return RedirectToAction("Index");
                }

                Console.WriteLine($"Conversation found: {conversation.Subject}");
                // Mesaj okunmamışsa okundu olarak işaretle
                await _messageService.MarkAsReadAsync(messageId, userId);

                return View(conversation);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Unauthorized access: {ex.Message}");
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Conversation action: {ex.Message}");
                TempData["Error"] = $"Could not load conversation: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Konuşmada cevap gönder
        [HttpPost]
        public async Task<IActionResult> SendReply(Guid conversationId, string content)
        {
            if (IsRateLimited("send-reply", 20, TimeSpan.FromMinutes(5)))
            {
                return Json(new { success = false, error = "You are sending replies too frequently. Please wait." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, error = "Reply content is required." });
            }

            try
            {
                var userId = GetRequiredUserId();
                var reply = await _messageService.SendReplyAsync(conversationId, content.Trim(), userId);
                
                if (reply != null)
                {
                    return Json(new { 
                        success = true, 
                        message = "Reply sent successfully!",
                        reply = new {
                            id = reply.Id,
                            content = reply.Content,
                            senderUsername = reply.SenderUsername,
                            sentAt = reply.SentAt,
                            relativeSentTime = reply.RelativeSentTime
                        }
                    });
                }
                
                return Json(new { success = false, error = "Failed to send reply." });
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, error = "You need to be logged in." });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An error occurred while sending the reply." });
            }
        }
    }
}
