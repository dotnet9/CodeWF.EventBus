using Messages.Commands;
using Messages.Queries;

namespace Messages.Services
{
    public interface IProductService
    {
        void CreateProduct(CreateProductCommand command);
        void DeleteProduct(DeleteProductCommand command);
        void QueryProduct(ProductsQuery query);
    }
}