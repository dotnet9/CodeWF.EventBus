namespace MessageViewModel
{
    public class DeleteProductMessage : CodeWF.EventBus.Message
    {
        public string Id { get; }

        public DeleteProductMessage(object sender, string id) : base(sender)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"删除产品消息-》产品Id：{Id}";
        }
    }
}