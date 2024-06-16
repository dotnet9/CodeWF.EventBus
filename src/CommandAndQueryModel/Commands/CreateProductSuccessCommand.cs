using CodeWF.EventBus;

namespace CommandAndQueryModel.Commands
{
    public class CreateProductSuccessCommand : Command
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}