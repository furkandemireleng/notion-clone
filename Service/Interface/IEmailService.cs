using SendGrid;

namespace notion_clone.Service.Interface;

public interface IEmailService
{
    Task<Response> SendEmailAsync(string to, string subject, string plainTextContent, string htmlContent);
}