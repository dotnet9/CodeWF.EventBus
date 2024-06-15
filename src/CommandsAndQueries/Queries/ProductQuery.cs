using CodeWF.EventBus;
using CommandsAndQueries.Dto;
using System;

namespace CommandsAndQueries.Queries
{
    public class ProductQuery : Query<ProductItemDto>
    {
        public Guid ProductId { get; set; }
        public override ProductItemDto Result { get; set; }
    }
}