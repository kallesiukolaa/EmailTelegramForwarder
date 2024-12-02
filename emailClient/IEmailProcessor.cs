using System.Threading.Tasks;
using MimeKit;

namespace EmailToTelegramBot
{
    // 4. IEmailProcessor - Interface for email processing.
    public interface IEmailProcessor
    {
        Task ForwardEmailAsync(MimeMessage email);
        Task<bool> ShouldForwardEmailAsync(MimeMessage email);
    }
}
