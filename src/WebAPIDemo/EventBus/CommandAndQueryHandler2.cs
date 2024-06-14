using CodeWF.EventBus;
using Messages.Commands;
using Messages.Queries;
using Messages.Services;
using WebAPIDemo.Services;

namespace WebAPIDemo.EventBus
{
    //[Event]
    public class CommandAndQueryHandler2
    {
        private readonly ITimeService timeService;
        private readonly IProductService _productService;

        public CommandAndQueryHandler2(ITimeService timeService, IProductService productService)
        {
            this.timeService = timeService;
            _productService = productService;
        }

        [EventHandler(Order = 3)]
        public void ReceiveAutoCreateProductCommand3(CreateProductCommand command)
        {
            AddLog($"CommandAndQueryHandler2 Received command 3 \"{command}\"");
            _productService.CreateProduct(command);
        }

        [EventHandler(Order = 1)]
        public void ReceiveAutoDeleteProductCommand(DeleteProductCommand command)
        {
            AddLog($"CommandAndQueryHandler2 Received command \"{command}\"");
            _productService.DeleteProduct(command);
        }

        [EventHandler(Order = 2)]
        public void ReceiveAutoCreateProductCommand2(CreateProductCommand command)
        {
            AddLog($"CommandAndQueryHandler2 Received command 2 \"{command}\"");
            _productService.CreateProduct(command);
        }

        [EventHandler(Order = 2)]
        public void ReceiveAutoProductsQuery(ProductsQuery query)
        {
            AddLog($"CommandAndQueryHandler2 Received query \"{query}\"");
            _productService.QueryProduct(query);
            AddLog($"After query \"{query}\"");
        }

        private void AddLog(string message)
        {
            Console.WriteLine($"{timeService.GetTime()}: {message}\r\n");
        }
    }
}