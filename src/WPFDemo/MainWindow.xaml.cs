using CodeWF.EventBus;
using System.Windows;
using WPFDemo.EventBus;

namespace WPFDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendMessage_OnClick(object sender, RoutedEventArgs e)
        {
            AddLog($"发送\"{this.TxtMessage.Text}\"");
            Messenger.Default.Publish(this, new SayHelloMessage(this, this.TxtMessage.Text));
        }

        private void AddLog(string message)
        {
            this.TxtLog.AppendText($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }

        private void ManuSubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Subscribe<SayHelloMessage>(this, ReceiveManuMessage);
        }

        private void AutoSubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Subscribe(this);
        }

        private void ManuUnsubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Unsubscribe<SayHelloMessage>(this, ReceiveManuMessage);
        }

        private void AutoUnsubscribe_OnClick(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Unsubscribe(this);
        }

        #region 消息处理方法

        private void ReceiveManuMessage(SayHelloMessage message)
        {
            AddLog($"收到手工订阅消息“{message.Word}”");
        }

        [EventHandler(Order = 2)]
        private void ReceiveAutoMessage2(SayHelloMessage message)
        {
            AddLog($"收到自动订阅消息({nameof(ReceiveAutoMessage2)})“{message.Word}”");
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoMessage1(SayHelloMessage message)
        {
            AddLog($"收到自动订阅消息({nameof(ReceiveAutoMessage1)})“{message.Word}”");
        }

        [EventHandler(Order = 3)]
        private void ReceiveAutoMessage3(SayHelloMessage message)
        {
            AddLog($"收到自动订阅消息({nameof(ReceiveAutoMessage3)})“{message.Word}”");
        }

        #endregion
    }
}