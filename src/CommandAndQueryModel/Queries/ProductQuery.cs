using CodeWF.EventBus;
using CommandAndQueryModel.Dto;

namespace CommandAndQueryModel.Queries
{
    public class ProductQuery : Query<ProductItemDto>
    {
        public Guid ProductId { get; set; }
        public override ProductItemDto Result { get; set; }
    }
}