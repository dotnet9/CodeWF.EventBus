using System;
using System.Reflection;

namespace CodeWF.EventBus
{
    public class WeakActionAndToken
    {
        public Type RecipientType { get; set; }
        public Delegate Action { get; set; }

        public int Order { get; set; }
    }

    public class WeakMethod
    {
        public Type RecipientType { get; set; }
        public MethodInfo Method { get; set; }

        public int Order { get; set; }
    }
}