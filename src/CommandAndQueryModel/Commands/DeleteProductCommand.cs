using CodeWF.EventBus;

namespace CommandAndQueryModel.Commands
{
    public class DeleteProductCommand : Command
    {
        public Guid ProductId { get; set; }
    }
}