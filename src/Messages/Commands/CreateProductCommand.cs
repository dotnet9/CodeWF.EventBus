using CodeWF.EventBus;

namespace Messages.Commands
{
    public class CreateProductCommand : Command
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return $"Create product command ->Product name:{Name}, price: {Price}";
        }
    }
}