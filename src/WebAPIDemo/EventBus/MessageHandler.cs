using CodeWF.EventBus;
using MessageViewModel;

namespace WebAPIDemo.EventBus
{
    public class MessageHandler
    {
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
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }
    }
}