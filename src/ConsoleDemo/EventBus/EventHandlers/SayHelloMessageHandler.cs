using CodeWF.EventBus;
using ConsoleDemo.EventBus.Events;

namespace ConsoleDemo.EventBus.EventHandlers
{
    internal class SayHelloMessageHandler
    {
        internal SayHelloMessageHandler()
        {
            Messenger.Default.Subscribe<SayHelloMessage>(this, ReceiveSayHelloMessage);
            Messenger.Default.Subscribe(this);
            Messenger.Default.Publish(this, new SayHelloMessage(this, "Hello!"));
        }

        void ReceiveSayHelloMessage(SayHelloMessage message)
        {
            AddLog($"Receive SayHelloMessage, message is: {message.Word}");
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

        private void AddLog(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }
    }
}