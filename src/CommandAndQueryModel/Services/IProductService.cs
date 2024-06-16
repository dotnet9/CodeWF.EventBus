using CommandAndQueryModel.Dto;

namespace CommandAndQueryModel.Services
{
    public interface IProductService
    {
        Task<bool> AddProductAsync(CreateProductRequest request);

        Task<bool> RemoveProductAsync(Guid productId);

        Task<ProductItemDto> QueryProductAsync(Guid productId);

        Task<List<ProductItemDto>> QueryProductsAsync(string name);
    }
}