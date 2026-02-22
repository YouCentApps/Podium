using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Localization;
using Podium.Shared;

namespace Podium.Api.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationCode);
}

public class EmailService : IEmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly IStringLocalizer<ApiMessages> _localizer;

    public EmailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword,
        string senderEmail, string senderName, IStringLocalizer<ApiMessages> localizer)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
        _senderEmail = senderEmail;
        _senderName = senderName;
        _localizer = localizer;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationCode)
    {
        var subject = _localizer["Email_Subject"].Value;
        var header = _localizer["Email_Header"].Value;
        var title = _localizer["Email_Title"].Value;
        var body = _localizer["Email_Body"].Value;
        var expiry = _localizer["Email_Expiry"].Value;
        var ignore = _localizer["Email_Ignore"].Value;
        var automated = _localizer["Email_Automated"].Value;
        var copyright = _localizer["Email_Copyright"].Value;

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                IsBodyHtml = true,
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: #2563eb; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                            .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
                            .code {{ font-size: 32px; font-weight: bold; color: #2563eb; text-align: center; letter-spacing: 8px; padding: 20px; background: white; border-radius: 8px; margin: 20px 0; }}
                            .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>{header}</h1>
                            </div>
                            <div class='content'>
                                <h2>{title}</h2>
                                <p>{body}</p>
                                <div class='code'>{verificationCode}</div>
                                <p><strong>{expiry}</strong></p>
                                <p>{ignore}</p>
                            </div>
                            <div class='footer'>
                                <p>{automated}</p>
                                <p>&copy; {DateTime.Now.Year} Podium. {copyright}</p>
                            </div>
                        </div>
                    </body>
                    </html>
                "
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email to {toEmail}: {ex.Message}");
            throw;
        }
    }
}
