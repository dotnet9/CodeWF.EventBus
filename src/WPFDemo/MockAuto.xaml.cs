using CodeWF.EventBus;
using Messages;
using System.Windows;

namespace WPFDemo
{
    public partial class MockAuto : Window
    {
        private bool _isSubscribed;

        public MockAuto()
        {
            InitializeComponent();
            ChangeSubscribe();
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            AddLog($"Publish \"{this.TxtMessage.Text}\"");
            Messenger.Default.Publish(this, new DeleteProductMessage(this, this.TxtMessage.Text));
        }

        private void SubscribeOrUnsubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeSubscribe();
        }

        private void ChangeSubscribe()
        {
            _isSubscribed = !_isSubscribed;
            if (_isSubscribed)
            {
                BtnEvent.Content = "Unsubscribe Message";
                Messenger.Default.Subscribe(this);
            }
            else
            {
                BtnEvent.Content = "Subscribe Message";
                Messenger.Default.Unsubscribe(this);
            }
        }

        private void AddLog(string message)
        {
            this.TxtLog.AppendText($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }


        [EventHandler(Order = 2)]
        private void ReceiveAutoMessage2(CreateProductMessage message)
        {
            AddLog($"Received automatic subscription message 2 \"{message}\"");
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoMessage1(CreateProductMessage message)
        {
            AddLog($"Received automatic subscription message 1 \"{message}\"");
        }
    }
}