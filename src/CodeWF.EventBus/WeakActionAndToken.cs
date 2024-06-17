using System;

namespace CodeWF.EventBus
{
    public class WeakActionAndToken
    {
        public Type RecipientType { get; set; }
        public Delegate Action { get; set; }

        public int Order { get; set; }
    }
}