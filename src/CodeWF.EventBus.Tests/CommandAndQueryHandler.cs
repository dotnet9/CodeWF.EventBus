using CommandAndQueryModel.Commands;
using CommandAndQueryModel.Dto;
using CommandAndQueryModel.Queries;
using CommandAndQueryModel.Services;

namespace CodeWF.EventBus.Tests
{
    internal class CommandAndQueryHandler
    {
        private readonly IProductService _productService = ProductService.Default;

        [EventHandler]
        private async Task ReceiveCreateProductCommandAsync(CreateProductCommand command)
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
        private async Task ReceiveCreateProductSuccessCommandSendEmailAsync(CreateProductSuccessCommand command)
        {
            Console.WriteLine($"Now send email notify create product success, name is = {command.Name}");
            await Task.CompletedTask;
        }

        [EventHandler(Order = 1)]
        private async Task ReceiveCreateProductSuccessCommandSendSmsAsync(CreateProductSuccessCommand command)
        {
            Console.WriteLine($"Now send sms notify create product success, name is = {command.Name}");
            await Task.CompletedTask;
        }

        [EventHandler(Order = 3)]
        private void ReceiveCreateProductSuccessCommandCallPhone(CreateProductSuccessCommand command)
        {
            Console.WriteLine($"Now call phone notify create product success, name is = {command.Name}");
        }

        [EventHandler]
        private async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
        {
            var isRemoveSuccess = await _productService.RemoveProductAsync(command.ProductId);
            Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
        }

        [EventHandler]
        private async Task ReceiveProductQueryAsync(ProductQuery query)
        {
            var product = await _productService.QueryProductAsync(query.ProductId);
            query.Result = product;
        }

        [EventHandler]
        private async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
        {
            var products = await _productService.QueryProductsAsync(query.Name);
            query.Result = products;
        }
    }
}