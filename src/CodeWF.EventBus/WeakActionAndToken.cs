using System;

namespace CodeWF.EventBus
{
    public class WeakActionAndToken
    {
        public object Recipient { get; set; }

        public Delegate Action { get; set; }

        public void ExecuteWithObject<TMessage>(TMessage message) where TMessage : Message
        {
            if (Action is Action<TMessage> factAction) factAction.Invoke(message);
        }
    }
}