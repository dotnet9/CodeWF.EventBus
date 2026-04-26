using CodeWF.EventBus;

namespace CommandAndQueryModel.Commands
{
    public class CreateProductSuccessCommand : Command
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
