using CommandsAndQueries.Commands;
using CommandsAndQueries.Dto;
using CommandsAndQueries.Queries;
using CommandsAndQueries.Services;

namespace CodeWF.EventBus.Tests
{
    internal class CommandAndQueryHandler
    {
        private readonly IProductService _productService = ProductService.Default;

        [EventHandler]
        public async Task ReceiveCreateProductCommandAsync(CreateProductCommand command)
        {
            var isAddSuccess = await _productService.AddProductAsync(new CreateProductRequest()
                { Name = command.Name, Price = command.Price });
            if (isAddSuccess)
            {
                await EventBus.Default.PublishAsync(this,
                    new CreateProductSuccessCommand() { Name = command.Name, Price = command.Price });
            }
            else
            {
                Console.WriteLine("Create product fail");
            }
        }

        [EventHandler(Order = 2)]
        public async Task ReceiveCreateProductSuccessCommandSendEmailAsync(CreateProductSuccessCommand command)
        {
            Console.WriteLine($"Now send email notify create product success, name is = {command.Name}");
            await Task.CompletedTask;
        }

        [EventHandler(Order = 1)]
        public async Task ReceiveCreateProductSuccessCommandSendSmsAsync(CreateProductSuccessCommand command)
        {
            Console.WriteLine($"Now send sms notify create product success, name is = {command.Name}");
            await Task.CompletedTask;
        }

        [EventHandler(Order = 3)]
        public void ReceiveCreateProductSuccessCommandCallPhone(CreateProductSuccessCommand command)
        {
            Console.WriteLine($"Now call phone notify create product success, name is = {command.Name}");
        }

        [EventHandler]
        public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
        {
            var isRemoveSuccess = await _productService.RemoveProductAsync(command.ProductId);
            Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
        }

        [EventHandler]
        public async Task ReceiveProductQueryAsync(ProductQuery query)
        {
            var product = await _productService.QueryProductAsync(query.ProductId);
            query.Result = product;
        }

        [EventHandler]
        public async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
        {
            var products = await _productService.QueryProductsAsync(query.Name);
            query.Result = products;
        }
    }
}