namespace Messages
{
    public class CreateProductMessage : CodeWF.EventBus.Message
    {
        public string Name { get; }

        public CreateProductMessage(object sender, string name) : base(sender)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"Create product message ->Product name:{Name}";
        }
    }
}