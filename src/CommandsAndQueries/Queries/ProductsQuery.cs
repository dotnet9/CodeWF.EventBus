using CodeWF.EventBus;
using CommandsAndQueries.Dto;
using System.Collections.Generic;

namespace CommandsAndQueries.Queries
{
    public class ProductsQuery : Query<List<ProductItemDto>>
    {
        public string Name { get; set; }
        public override List<ProductItemDto> Result { get; set; }
    }
}