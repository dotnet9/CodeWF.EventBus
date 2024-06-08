using System;

namespace CodeWF.EventBus
{
    public abstract class Message
    {
        public object Sender { get; }

        public Message(object sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }
    }
}