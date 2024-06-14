using System;
using Messages.Commands;
using Messages.Dto;
using Messages.Models;
using Messages.Queries;
using System.Collections.Generic;
using System.Linq;

namespace Messages.Services
{
    public class ProductService : IProductService
    {
        public static readonly ProductService Default = new ProductService();

        private List<ProductItem> _products = new List<ProductItem>();

        public void CreateProduct(CreateProductCommand command)
        {
            _products.Add(new ProductItem()
            {
                Name = command.Name,
                Price = command.Price
            });
            ShowInfo();
        }

        public void DeleteProduct(DeleteProductCommand command)
        {
            _products.RemoveAll(product => product.Name == command.Name);
            ShowInfo();
        }

        public void QueryProduct(ProductsQuery query)
        {
            query.Result = _products.Where(product => product.Name.Contains(query.Name)).ToList().ConvertAll(product =>
                new ProductItemDto()
                {
                    Name = product.Name,
                    Price = product.Price
                });
            ShowInfo();
        }

        private void ShowInfo()
        {
            Console.WriteLine($"Product count: {_products.Count}");
        }
    }
}