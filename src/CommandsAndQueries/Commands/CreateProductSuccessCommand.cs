using CodeWF.EventBus;

namespace CommandsAndQueries.Commands
{
    public class CreateProductSuccessCommand : Command
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}