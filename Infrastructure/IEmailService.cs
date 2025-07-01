using System.Net.Mail;
using System.Net;

namespace Eryth.Infrastructure
{
    // Email gönderme servisi için arayüz
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendWelcomeEmailAsync(string to, string username);
        Task SendPasswordResetEmailAsync(string to, string resetToken);
        Task SendEmailVerificationAsync(string to, string verificationToken);
    }
}
