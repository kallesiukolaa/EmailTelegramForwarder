using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace EmailToTelegramBot
{
    public class ImapEmailClient : IEmailClient
    {
        private readonly string _imapServer;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private ImapClient _client;
        private CancellationTokenSource _cancellationTokenSource;
        private int _inboxCount;
        private int _cancelTokenAfterMinutes;

        public event EventHandler<MimeMessage> OnNewEmailReceived;

        public ImapEmailClient(string imapServer, int port, string username, string password, int maxTokenLifetimeMinutes)
        {
            _imapServer = imapServer;
            _port = port;
            _username = username;
            _password = password;
            _cancelTokenAfterMinutes = maxTokenLifetimeMinutes;
        }

        public async Task ConnectAsync()
        {
            _client = new ImapClient();
            await _client.ConnectAsync(_imapServer, _port, SecureSocketOptions.None);
            _client.AuthenticationMechanisms.Remove("NTLM");
            await _client.AuthenticateAsync(_username, _password);
        }

        public async Task StartListeningAsync()
        {
            IMailFolder inbox = _client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            inbox.Closed += OnInboxClosed;
            _inboxCount = inbox.Count();
            inbox.MessageExpunged += OnMessageExpunged;
            inbox.CountChanged += OnMessageCountChanged;
            _client.Disconnected += OnDisconnected;
            
            await Idle();
        }

        public async Task Idle() {
            do {
                RenewToken();
                if (!_client.Inbox.IsOpen){
                    await _client.Inbox.OpenAsync(FolderAccess.ReadOnly);
                }
                await _client.IdleAsync(_cancellationTokenSource.Token);
                HandleMessages();
            }while (true);
            
        }

        private void HandleMessages() {
            IEnumerable<MimeMessage> newMessages = _client.Inbox.Skip(_inboxCount);
            if (newMessages.Count() > 0) {
                _inboxCount = _client.Inbox.Count;
                foreach(MimeMessage message in newMessages) {
                    OnNewEmailReceived(this, message);
                }
            }
        }

        private void OnMessageExpunged(object sender, MessageEventArgs e)
        {
            _inboxCount--;
        }

        private async void OnInboxClosed(object sender, EventArgs e)
        {
            await _client.Inbox.OpenAsync(FolderAccess.ReadOnly);
        }

        private void OnMessageCountChanged(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            Console.WriteLine("New message arrived!");
        }

        private async void OnDisconnected(object sender, EventArgs e) {
            await ConnectAsync();
        }

        private void RenewToken() {
            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes (_cancelTokenAfterMinutes));
            Console.WriteLine("Token renewed.");
        }
    }
}
