using CommandsAndQueries.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandsAndQueries.Services
{
    public interface IProductService
    {
        Task<bool> AddProductAsync(CreateProductRequest request);

        Task<bool> RemoveProductAsync(Guid productId);

        Task<ProductItemDto> QueryProductAsync(Guid productId);

        Task<List<ProductItemDto>> QueryProductsAsync(string name);
    }
}