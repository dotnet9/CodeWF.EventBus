using CodeWF.EventBus;
using Messages;
using System.Windows;

namespace WPFDemo
{
    public partial class MockManu : Window
    {
        private bool _isSubscribed;

        public MockManu()
        {
            InitializeComponent();
            ChangeSubscribe();
        }

        void ReceiveManuDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"Received manually registered \"{message}|");
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            AddLog($"Publish \"{this.TxtMessage.Text}\"");
            Messenger.Default.Publish(this, new CreateProductMessage(this, this.TxtMessage.Text));
        }

        private void AddLog(string message)
        {
            this.TxtLog.AppendText($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
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
                Messenger.Default.Subscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
            }
            else
            {
                BtnEvent.Content = "Subscribe Message";
                Messenger.Default.Unsubscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
            }
        }
    }
}