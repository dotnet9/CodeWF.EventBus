using CodeWF.EventBus;
using MessageViewModel;
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
            AddLog($"收到手动注册的{message}");
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            AddLog($"发送\"{this.TxtMessage.Text}\"");
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
                BtnEvent.Content = "注销消息";
                Messenger.Default.Subscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
            }
            else
            {
                BtnEvent.Content = "注册消息";
                Messenger.Default.Unsubscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
            }
        }
    }
}