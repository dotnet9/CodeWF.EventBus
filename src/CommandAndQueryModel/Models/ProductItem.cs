using System;

namespace CommandAndQueryModel.Models
{
    public class ProductItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
