using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MimeKit;

namespace EmailToTelegramBot
{
    // 3. EmailProcessor - Handles email conditions and forwards them to Telegram.
    public class EmailProcessor : IEmailProcessor
    {
        private readonly ITelegramNotifier _telegramNotifier;
        private readonly string _emailFromRegexPattern;

        public EmailProcessor(ITelegramNotifier telegramNotifier, string emailFromRegexPattern)
        {
            _telegramNotifier = telegramNotifier;
            _emailFromRegexPattern = emailFromRegexPattern;
        }

        public async Task<bool> ShouldForwardEmailAsync(MimeMessage email)
        {
            // Check your conditions here, e.g., specific subject, sender, etc.
            // Example condition: if the subject contains "Important"
            //return email.Subject.Contains("Important");
            //return true;
            return Regex.Match(email.From.Mailboxes.First().Address, _emailFromRegexPattern, RegexOptions.IgnoreCase).Success;
        }

        public async Task ForwardEmailAsync(MimeMessage email)
        {
            // Forward the email to a Telegram channel
            await _telegramNotifier.SendMessageAsync(email);
        }
    }
}
