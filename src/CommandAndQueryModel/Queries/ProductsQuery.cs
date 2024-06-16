using CodeWF.EventBus;
using CommandAndQueryModel.Dto;

namespace CommandAndQueryModel.Queries
{
    public class ProductsQuery : Query<List<ProductItemDto>>
    {
        public string Name { get; set; }
        public override List<ProductItemDto> Result { get; set; }
    }
}