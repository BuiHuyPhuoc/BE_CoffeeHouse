namespace CoffeeHouseAPI.Services.Email
{
    public interface IEmailSender
    {
        
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailAsync(List<string> recipients, string subject, string message, List<string>? cc = null, List<string>? bcc = null);

    }
}
