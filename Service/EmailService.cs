using SendGrid;
using SendGrid.Helpers.Mail;
using notion_clone.Service.Interface;

namespace notion_clone.Service;

public class EmailService : IEmailService
{
    private readonly AppSettings setting;

    public EmailService(AppSettings setting) 
    {
        this.setting = setting;
    }

    public async Task<Response> SendEmailAsync(string to, string subject, string plainTextContent, string htmlContent)
    {
        var client = new SendGridClient(setting.SendGridApiKey);
        var from = new EmailAddress("info@neohub.com.tr", "NEOHUB Dijital Varlıklar");
        var toEmail = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent, htmlContent);
        return await client.SendEmailAsync(msg);
    }
}