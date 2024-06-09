using CodeWF.EventBus;

namespace MessageViewModel
{
    public class SayHelloMessage : Message
    {
        public string Word { get; }

        public SayHelloMessage(object sender, string word) : base(sender)
        {
            Word = word;
        }
    }
}