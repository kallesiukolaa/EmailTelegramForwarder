using System.Threading.Tasks;
using MimeKit;

namespace EmailToTelegramBot
{
    // 6. ITelegramNotifier - Interface for Telegram notification.
    public interface ITelegramNotifier
    {
        Task SendMessageAsync(MimeMessage email);
    }
}
