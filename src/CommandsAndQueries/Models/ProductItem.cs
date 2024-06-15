using System;

namespace CommandsAndQueries.Models
{
    public class ProductItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}