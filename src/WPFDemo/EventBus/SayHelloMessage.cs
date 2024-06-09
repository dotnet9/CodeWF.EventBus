using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDemo.EventBus
{
    public class SayHelloMessage : CodeWF.EventBus.Message
    {
        public string Word { get; }

        public SayHelloMessage(object sender, string word) : base(sender)
        {
            Word = word;
        }
    }
}