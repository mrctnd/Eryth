using System.Net.Mail;
using System.Net;

namespace Eryth.Infrastructure
{
    // SMTP email servisi implementasyonu
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]
                    ),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "", smtpSettings["FromName"] ?? "Eryth"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email başarıyla gönderildi: {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email gönderme hatası: {To}", to);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string username)
        {
            var subject = "Eryth'e Hoş Geldiniz!";
            var body = $@"
                <h2>Merhaba {username}!</h2>
                <p>Eryth müzik platformuna hoş geldiniz. Hesabınız başarıyla oluşturuldu.</p>
                <p>Şimdi müziklerini paylaşmaya ve keşfetmeye başlayabilirsin!</p>
                <p>İyi müzikler,<br>Eryth Ekibi</p>
            ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken)
        {
            var subject = "Şifre Sıfırlama";
            var resetUrl = $"{_configuration["BaseUrl"]}/auth/reset-password?token={resetToken}";
            var body = $@"
                <h2>Şifre Sıfırlama</h2>
                <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
                <p><a href='{resetUrl}'>Şifreyi Sıfırla</a></p>
                <p>Bu link 1 saat içinde geçerliliğini yitirecektir.</p>
                <p>Eğer bu isteği siz yapmadıysanız, bu emaili görmezden gelebilirsiniz.</p>
            ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailVerificationAsync(string to, string verificationToken)
        {
            var subject = "Email Doğrulama";
            var verificationUrl = $"{_configuration["BaseUrl"]}/auth/verify-email?token={verificationToken}";
            var body = $@"
                <h2>Email Adresinizi Doğrulayın</h2>
                <p>Hesabınızı aktifleştirmek için aşağıdaki linke tıklayın:</p>
                <p><a href='{verificationUrl}'>Email'i Doğrula</a></p>
                <p>Bu link 24 saat içinde geçerliliğini yitirecektir.</p>
            ";

            await SendEmailAsync(to, subject, body);
        }
    }
}
