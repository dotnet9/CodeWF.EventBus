using System;

namespace CodeWF.EventBus
{
    public interface IEventBus
    {
        void Subscribe(object recipient);

        void Subscribe<TMessage>(object recipient, Action<TMessage> action) where TMessage : Command;

        void Unsubscribe(object recipient);

        void Unsubscribe<TMessage>(object recipient, Action<TMessage> action = null) where TMessage : Command;

        void Publish<TMessage>(object sender, TMessage message) where TMessage : Command;
    }
}