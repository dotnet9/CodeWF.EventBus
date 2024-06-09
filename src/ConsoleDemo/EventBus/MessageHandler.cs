using CodeWF.EventBus;
using MessageViewModel;

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
            Messenger.Default.Publish(this, new CreateProductMessage(this, $"{DateTime.Now:HHmmss}号产品"));
            Messenger.Default.Publish(this, new DeleteProductMessage(this, $"{DateTime.Now:HHmmss}号"));
        }

        void ReceiveManuCreateProductMessage(CreateProductMessage message)
        {
            AddLog($"收到手动注册的{message}");
        }

        void ReceiveManuDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"收到手动注册的{message}");
        }

        [EventHandler(Order = 3)]
        private void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
        {
            AddLog($"收到自动订阅消息3({message}”");
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
        {
            AddLog($"收到自动订阅消息({message}”");
        }

        [EventHandler(Order = 2)]
        private void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
        {
            AddLog($"收到自动订阅消息2({message}”");
        }

        private void AddLog(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }
    }
}