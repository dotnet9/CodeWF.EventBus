using CodeWF.EventBus;
using Messages.Dto;
using System.Collections.Generic;

namespace Messages.Queries
{
    public class ProductsQuery : Query<List<ProductItemDto>>
    {
        public string Name { get; set; }
        public override List<ProductItemDto> Result { get; set; }

        public override string ToString()
        {
            return $"Query product->Condition product name:{Name}, query result is {Result?.Count ?? 0}";
        }
    }
}