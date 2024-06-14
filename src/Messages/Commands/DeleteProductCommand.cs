using CodeWF.EventBus;

namespace Messages.Commands
{
    public class DeleteProductCommand : Command
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Delete product command ->Product name：{Name}";
        }
    }
}