namespace CommandAndQueryModel.Dto
{
    public class ProductItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
