using CodeWF.EventBus;
using Messages;

namespace ConsoleDemo.EventBus
{
    internal class MessageHandler
    {
        internal void ManuSubscribe()
        {
            Messenger.Default.Subscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
            Messenger.Default.Subscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
        }

        internal void ManuUnsubscribe()
        {
            Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
            Messenger.Default.Unsubscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
        }

        internal void AutoSubscribe()
        {
            Messenger.Default.Subscribe(this);
        }

        internal void AutoUnsubscribe()
        {
            Messenger.Default.Unsubscribe(this);
        }

        internal void Publish()
        {
            Messenger.Default.Publish(this, new CreateProductMessage(this, "Xiaomi 14 Ultra"));
            Messenger.Default.Publish(this, new DeleteProductMessage(this, "Matebook X Pro 2024"));
        }

        void ReceiveManuCreateProductMessage(CreateProductMessage message)
        {
            AddLog($"Received manually registered \"{message}\"");
        }

        void ReceiveManuDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"Received manually registered \"{message}\"");
        }

        [EventHandler(Order = 3)]
        private void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
        {
            AddLog($"Received automatic subscription message 3 \"({message}\"");
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"Received automatic subscription message \"{message}\"");
        }

        [EventHandler(Order = 2)]
        private void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
        {
            AddLog($"Received automatic subscription message 2 \"{message}\"");
        }

        private void AddLog(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }
    }
}