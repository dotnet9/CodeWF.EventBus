using CodeWF.EventBus;
using Messages;
using WebAPIDemo.Services;

namespace WebAPIDemo.EventBus
{
    //[Event]
    public class MessageHandler2
    {
        private readonly ITimeService timeService;

        public MessageHandler2(ITimeService timeService)
        {
            this.timeService = timeService;
        }

        [EventHandler(Order = 3)]
        public void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
        {
            AddLog($"MessageHandler2 Received message 3 \"{message}\"");
        }

        [EventHandler(Order = 1)]
        public void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"MessageHandler2 Received message \"{message}\"");
        }

        [EventHandler(Order = 2)]
        public void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
        {
            AddLog($"MessageHandler2 Received message 2 \"{message}\"");
        }

        private void AddLog(string message)
        {
            Console.WriteLine($"{timeService.GetTime()}: {message}\r\n");
        }
    }
}