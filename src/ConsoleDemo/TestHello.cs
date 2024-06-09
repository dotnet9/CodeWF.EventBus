using CodeWF.EventBus;
using MessageViewModel;

namespace ConsoleDemo
{
    internal class TestHello
    {
        internal TestHello()
        {
            Messenger.Default.Subscribe<SayHelloMessage>(this, ReceiveSayHelloMessage);
            Messenger.Default.Subscribe(this);
            Messenger.Default.Publish(this, new SayHelloMessage(this, "Hello!"));
        }

        void ReceiveSayHelloMessage(SayHelloMessage message)
        {
            Console.WriteLine($"Receive SayHelloMessage, message is: {message.Word}");
        }

        [EventHandler(Order = 1)]
        void ReceiveMessage2(SayHelloMessage message)
        {
            Console.WriteLine($"Receive auto handler 1, message is: {message}");
        }

        [EventHandler(Order = 2)]
        void ReceiveMessage3(SayHelloMessage message)
        {
            Console.WriteLine($"Receive auto handler 2, message is: {message}");
        }
    }
}