using System;

namespace CodeWF.EventBus
{
    public class WeakActionAndToken
    {
        public Delegate Action { get; set; }

        public int Order { get; set; }
    }
}