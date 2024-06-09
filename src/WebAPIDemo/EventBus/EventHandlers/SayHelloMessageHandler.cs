using CodeWF.EventBus;
using WebAPIDemo.EventBus.Events;

namespace WebAPIDemo.EventBus.EventHandlers
{
    public class SayHelloMessageHandler
    {
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