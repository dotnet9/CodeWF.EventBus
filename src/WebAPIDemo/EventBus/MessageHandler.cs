using CodeWF.EventBus;
using MessageViewModel;
using WebAPIDemo.Services;

namespace WebAPIDemo.EventBus
{
    public class MessageHandler
    {
        private readonly ITimeService timeService;

        public MessageHandler(ITimeService timeService)
        {
            this.timeService = timeService;
        }

        [EventHandler(Order = 3)]
        public void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
        {
            AddLog($"收到消息3({message}”");
        }

        [EventHandler(Order = 1)]
        public void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"收到消息({message}”");
        }

        [EventHandler(Order = 2)]
        public void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
        {
            AddLog($"收到消息2({message}”");
        }

        private void AddLog(string message)
        {
            Console.WriteLine($"{timeService.GetTime()}: {message}\r\n");
        }
    }
}