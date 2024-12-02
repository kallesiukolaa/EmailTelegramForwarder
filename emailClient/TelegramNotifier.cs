using System.Net.Http;
using System.Threading.Tasks;
using MimeKit;
using Telegram.Bot;
using Telegram.Bot.Types;
using CoreHtmlToImage;

namespace EmailToTelegramBot
{
    // 5. TelegramNotifier - Sends messages to a Telegram channel
    public class TelegramNotifier : ITelegramNotifier
    {
        private readonly string _telegramChannelId;
        private readonly TelegramBotClient _telegramBotClient;
        private readonly HtmlConverter _htmlConverter = new HtmlConverter();

        public TelegramNotifier(string botToken, string channelId)
        {
            _telegramChannelId = channelId;
            _telegramBotClient = new TelegramBotClient(botToken);
        }

        public async Task SendMessageAsync(MimeMessage email)
        {
            // Send the email content to the Telegram channel
            string from = email.From.Mailboxes.FirstOrDefault().Address;
            string subject = email.Subject ?? "";
            string body = ((TextPart)email.Body).Text ?? "Undefined message!";
            string message = $"New email received:\nSubject: {subject}\nFrom: {from}\nBody: {body}";
            await SendTelegramMessageAsync(message);
            //TODO Exception handling and logging in case the html is not valid
            await SendTelegramImageAsync( ConvertHTMLToStream(body));
        }

        private async Task SendTelegramMessageAsync(string message)
        {
            // Send a message to the Telegram channel using the Bot API.
            int chunkSize = 3000;

            IEnumerable<string> SplitString(string str, int size) => 
                Enumerable.Range(0, (int)Math.Ceiling(str.Length / (float)size))
                    .Select(i => str.Substring(i * size, Math.Min(str.Length - (i * size), size)));
            foreach(string submessage in SplitString(message, chunkSize)) {
                await _telegramBotClient.SendMessage(_telegramChannelId, submessage); // TODO applyy speacial regex for special sender
                Thread.Sleep(500);
            }
            
            /*
            var url = $"https://api.telegram.org/bot{_telegramBotToken}/sendMessage?chat_id={_telegramChannelId}&text={System.Uri.EscapeDataString(message)}";

            using (var httpClient = new HttpClient())
            {
                await httpClient.GetAsync(url);
            }
            */
        }

        private async Task SendTelegramImageAsync(Stream imageStream) {
            await _telegramBotClient.SendPhoto(_telegramChannelId, new InputFileStream(){
                Content = imageStream
            });
        }

        private Stream ConvertHTMLToStream(string htmlContent) {
            var bytes = _htmlConverter.FromHtmlString(htmlContent);
            return new MemoryStream(bytes);
        }
    }
}
