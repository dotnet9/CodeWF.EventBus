using CodeWF.EventBus;

namespace CommandAndQueryModel.Commands
{
    public class CreateProductCommand : Command
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
