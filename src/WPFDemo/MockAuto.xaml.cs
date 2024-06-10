using CodeWF.EventBus;
using MessageViewModel;
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
            AddLog($"发送\"{this.TxtMessage.Text}\"");
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
                BtnEvent.Content = "注销消息";
                Messenger.Default.Subscribe(this);
            }
            else
            {
                BtnEvent.Content = "注册消息";
                Messenger.Default.Unsubscribe(this);
            }
        }

        private void AddLog(string message)
        {
            this.TxtLog.AppendText($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }


        #region 消息处理方法

        [EventHandler(Order = 2)]
        private void ReceiveAutoMessage2(CreateProductMessage message)
        {
            AddLog($"收到自动订阅消息2“{message}”");
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoMessage1(CreateProductMessage message)
        {
            AddLog($"收到自动订阅消息1“{message}”");
        }

        #endregion
    }
}