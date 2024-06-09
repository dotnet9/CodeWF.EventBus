namespace WPFDemoUseNuget.Message
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