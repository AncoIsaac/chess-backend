using MimeKit;
using MailKit.Net.Smtp;

namespace chess.Data.service
{
    public class EmailService(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        private static string GetHtmlTemplate(string code)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Email-resetPassword.html");
            var htmlContent = File.ReadAllText(templatePath);
            return htmlContent.Replace("{{CODE}}", code); // Reemplaza el marcador en el HTML
        }

        public async Task SendVerificationEmail(string toEmail, string verificationCode)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = emailSettings["SmtpPort"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort))
            {
                throw new InvalidOperationException("SMTP server or port is not configured.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Soporte", emailSettings["SenderEmail"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Código de Verificación";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GetHtmlTemplate(verificationCode) // Usa la plantilla HTML
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, int.Parse(smtpPort), false);
            await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
