using CodeWF.EventBus;
using Messages.Commands;
using Messages.Queries;
using Messages.Services;

namespace ConsoleDemo
{
    internal class CommandAndQueryHandler
    {
        internal void ManuSubscribe()
        {
            EventBus.Default.Subscribe<CreateProductCommand>(this, ReceiveManuCreateProductCommand);
            EventBus.Default.Subscribe<DeleteProductCommand>(this, ReceiveManuDeleteProductCommand);
            EventBus.Default.Subscribe<ProductsQuery>(this, ReceiveManuProductsQuery);
        }

        internal void ManuUnsubscribe()
        {
            EventBus.Default.Unsubscribe<CreateProductCommand>(this, ReceiveManuCreateProductCommand);
            EventBus.Default.Unsubscribe<DeleteProductCommand>(this, ReceiveManuDeleteProductCommand);
            EventBus.Default.Unsubscribe<ProductsQuery>(this, ReceiveManuProductsQuery);
        }

        internal void AutoSubscribe()
        {
            EventBus.Default.Subscribe(this);
        }

        internal void AutoUnsubscribe()
        {
            EventBus.Default.Unsubscribe(this);
        }

        internal void Publish()
        {
            EventBus.Default.Publish(this, new CreateProductCommand { Name = "Xiaomi 14 Ultra", Price = 9498 });
            EventBus.Default.Publish(this, new CreateProductCommand { Name = "HUAWEI MateBook X Pro", Price = 14999 });
            EventBus.Default.Publish(this, new DeleteProductCommand { Name = "Xiaomi 14 Ultra" });
            var query = new ProductsQuery() { Name = "HUAWEI" };
            EventBus.Default.Publish(this, query);
            AddLog($"The query result: \"{query}\"");
        }

        void ReceiveManuCreateProductCommand(CreateProductCommand command)
        {
            AddLog($"Received manually registered \"{command}\"");
            ProductService.Default.CreateProduct(command);
        }

        void ReceiveManuDeleteProductCommand(DeleteProductCommand command)
        {
            AddLog($"Received manually registered \"{command}\"");
            ProductService.Default.DeleteProduct(command);
        }

        void ReceiveManuProductsQuery(ProductsQuery query)
        {
            AddLog($"Received manually registered \"{query}\"");
            ProductService.Default.QueryProduct(query);
            AddLog($"After query \"{query}\"");
        }

        [EventHandler(Order = 3)]
        private void ReceiveAutoCreateProductCommand3(CreateProductCommand command)
        {
            AddLog($"Received automatic subscription \"({command}\"");
            ProductService.Default.CreateProduct(command);
        }

        [EventHandler(Order = 1)]
        private void ReceiveAutoDeleteProductCommand(DeleteProductCommand command)
        {
            AddLog($"Received automatic subscription \"{command}\"");
            ProductService.Default.DeleteProduct(command);
        }

        [EventHandler(Order = 2)]
        private void ReceiveAutoCreateProductCommand2(CreateProductCommand command)
        {
            AddLog($"Received automatic subscription2 \"{command}\"");
            ProductService.Default.CreateProduct(command);
        }

        [EventHandler(Order = 4)]
        private void ReceiveAutoProductsQuery(ProductsQuery query)
        {
            AddLog($"Received automatic subscription \"{query}\"");
            ProductService.Default.QueryProduct(query);
            AddLog($"After query \"{query}\"");
        }

        private void AddLog(string message)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
        }
    }
}