using CommandAndQueryModel.Dto;
using CommandAndQueryModel.Models;

namespace CommandAndQueryModel.Services
{
    public class ProductService : IProductService
    {
        public static readonly ProductService Default = new ProductService();
        private readonly List<ProductItem> _productItems = new List<ProductItem>();

        public async Task<bool> AddProductAsync(CreateProductRequest request)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            _productItems.Add(new ProductItem()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price
            });
            return true;
        }

        public async Task<bool> RemoveProductAsync(Guid productId)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            var removeCount = _productItems.RemoveAll(item => item.Id == productId);
            return removeCount > 0;
        }

        public async Task<ProductItemDto> QueryProductAsync(Guid productId)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            var productFromDb = _productItems.FirstOrDefault(item => item.Id == productId);
            if (productFromDb == null)
            {
                return null;
            }

            return new ProductItemDto()
            {
                Id = productFromDb.Id,
                Name = productFromDb.Name,
                Price = productFromDb.Price
            };
        }

        public async Task<List<ProductItemDto>> QueryProductsAsync(string name)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            var productSFromDb = _productItems.Where(item => item.Name.Contains(name)).ToList();
            if (!productSFromDb.Any())
            {
                return null;
            }

            return productSFromDb.Select(item => new ProductItemDto()
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price
            }).ToList();
        }
    }
}