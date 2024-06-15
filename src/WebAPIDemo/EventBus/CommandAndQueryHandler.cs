﻿using CodeWF.EventBus;
using CommandsAndQueries.Commands;
using CommandsAndQueries.Dto;
using CommandsAndQueries.Queries;
using CommandsAndQueries.Services;

namespace WebAPIDemo.EventBus
{
    [Event]
    public class CommandAndQueryHandler(IEventBus eventBus, IProductService productService)
    {
        [EventHandler]
        public async Task ReceiveCreateProductCommandAsync(CreateProductCommand command)
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
            var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
            Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
        }

        [EventHandler]
        public async Task ReceiveProductQueryAsync(ProductQuery query)
        {
            var product = await productService.QueryProductAsync(query.ProductId);
            query.Result = product;
        }

        [EventHandler]
        public async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
        {
            var products = await productService.QueryProductsAsync(query.Name);
            query.Result = products;
        }
    }
}