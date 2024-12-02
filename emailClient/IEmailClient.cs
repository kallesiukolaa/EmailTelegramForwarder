using System.Threading.Tasks;
using MimeKit;

namespace EmailToTelegramBot
{
    // 2. IEmailClient - Interface for interacting with the IMAP server
    public interface IEmailClient
    {
        event EventHandler<MimeMessage> OnNewEmailReceived;

        Task ConnectAsync();
        Task StartListeningAsync();
        Task Idle();
    }
}
