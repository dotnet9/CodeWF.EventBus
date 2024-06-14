using System;

namespace CodeWF.EventBus
{
    public class WeakActionAndToken
    {
        public object Recipient { get; set; }

        public Delegate Action { get; set; }

        public int Order { get; set; }

        public void ExecuteWithObject<TMessage>(TMessage message) where TMessage : Command
        {
            if (Action is Action<TMessage> factAction) factAction.Invoke(message);
        }
    }
}