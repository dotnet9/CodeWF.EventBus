using System;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public class WeakActionAndToken
    {
        public object Recipient { get; set; }

        public Delegate Action { get; set; }

        public int Order { get; set; }
    }
}