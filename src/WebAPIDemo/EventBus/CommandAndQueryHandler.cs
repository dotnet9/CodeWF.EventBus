using CodeWF.EventBus;
using CommandAndQueryModel.Commands;
using CommandAndQueryModel.Dto;
using CommandAndQueryModel.Queries;
using CommandAndQueryModel.Services;

namespace WebAPIDemo.EventBus
{
    [Event]
    public class CommandAndQueryHandler(IEventBus eventBus, IProductService productService)
    {
        [EventHandler]
        private async Task ReceiveCreateProductCommandAsync(CreateProductCommand command)
        {
            var isAddSuccess = await productService.AddProductAsync(new CreateProductRequest()
                { Name = command.Name, Price = command.Price });
            if (isAddSuccess)
            {
                await eventBus.PublishAsync(this,
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
            var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
            Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
        }

        [EventHandler]
        private async Task ReceiveProductQueryAsync(ProductQuery query)
        {
            var product = await productService.QueryProductAsync(query.ProductId);
            query.Result = product;
        }

        [EventHandler]
        private async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
        {
            var products = await productService.QueryProductsAsync(query.Name);
            query.Result = products;
        }
    }
}