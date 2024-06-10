namespace MessageViewModel
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
            return $"创建产品消息-》产品名称：{Name}";
        }
    }
}