using System;

namespace CodeWF.EventBus
{
    public interface IMessenger
    {
        void Subscribe(object recipient);

        void Subscribe<TMessage>(object recipient, Action<TMessage> action) where TMessage : Message;

        void Unsubscribe(object recipient);

        void Unsubscribe<TMessage>(object recipient, Action<TMessage> action = null) where TMessage : Message;

        void Publish<TMessage>(object sender, TMessage message) where TMessage : Message;
    }
}