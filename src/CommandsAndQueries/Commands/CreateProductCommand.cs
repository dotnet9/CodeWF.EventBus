using CodeWF.EventBus;

namespace CommandsAndQueries.Commands
{
    public class CreateProductCommand : Command
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}