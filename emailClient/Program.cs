using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EmailToTelegramBot
{
    
    class Config {
        public string imapHostName {get; set;}
        public int imapPort {get;set;}
        public string imapUserName {get; set;}
        public string imapPassword {get; set;}
        public string telegramBotToken {get;set;}
        public string telegramChannelID {get;set;}
        public string regexSenderWhenToForward {get;set;}
    }
    class Program
    {
        
        static Config GetConfig(){
            return new Config() {
                imapHostName = Environment.GetEnvironmentVariable("TW-IMAP-HOSTNAME"),
                imapPort = int.Parse(Environment.GetEnvironmentVariable("TW-IMAP-PORT") ?? "143"),
                imapUserName = Environment.GetEnvironmentVariable("TW-IMAP-USERNAME"),
                imapPassword = Environment.GetEnvironmentVariable("TW-IMAP-PASSWORD"),
                telegramBotToken = Environment.GetEnvironmentVariable("TW-TELEGRAM-BOT-TOKEN"),
                telegramChannelID = Environment.GetEnvironmentVariable("TW-TELEGRAM-CHANNEL-ID"),
                regexSenderWhenToForward = Environment.GetEnvironmentVariable("TW-SENDER-REGEX")
            };
        }
        static async Task Main(string[] args)
        {
            // Set up Dependency Injection (DI)
            Config conf = GetConfig();
            var imapEmailClient = new ImapEmailClient(conf.imapHostName, conf.imapPort, conf.imapUserName, conf.imapPassword, 9);
            var telegramNotifier = new TelegramNotifier(conf.telegramBotToken, conf.telegramChannelID);
            var emailProcessor = new EmailProcessor(telegramNotifier, conf.regexSenderWhenToForward);
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmailClient>(provider =>
                    imapEmailClient)
                .AddSingleton<ITelegramNotifier>(provider =>
                    telegramNotifier)
                .AddSingleton<IEmailProcessor, EmailProcessor>(provider => 
                    emailProcessor)
                .AddSingleton<EmailListenerService>()
                .BuildServiceProvider();

            // Get the service (EmailListenerService) to start the application
            var emailListenerService = serviceProvider.GetRequiredService<EmailListenerService>();

            // Start listening for incoming emails
            await emailListenerService.StartListeningAsync();

            Console.WriteLine("Email listener is running... Press any key to exit.");
            Console.ReadLine();
        }
    }
}
