using System;
using System.Threading.Tasks;

namespace EmailToTelegramBot
{
    // 1. EmailListenerService - Responsible for listening to the IMAP server.
    public class EmailListenerService
    {
        private readonly IEmailClient _emailClient;
        private readonly IEmailProcessor _emailProcessor;

        public EmailListenerService(IEmailClient emailClient, IEmailProcessor emailProcessor)
        {
            _emailClient = emailClient;
            _emailProcessor = emailProcessor;
        }

        public async Task StartListeningAsync()
        {
            // Connect to IMAP server
            await _emailClient.ConnectAsync();

            // Listen to incoming emails (using IMAP push notifications or polling)
            _emailClient.OnNewEmailReceived += async (sender, email) =>
            {
                // Process the received email
                bool shouldForward = await _emailProcessor.ShouldForwardEmailAsync(email);

                // If the email matches the conditions, forward it
                if (shouldForward)
                {
                    await _emailProcessor.ForwardEmailAsync(email);
                }

            };

            // Start the listener (keep running)
            await _emailClient.StartListeningAsync();
        }
    }
}
