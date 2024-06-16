using CommandAndQueryModel.Commands;
using CommandAndQueryModel.Queries;

namespace CodeWF.EventBus.Tests
{
    public class EventBusUnitTest
    {
        private readonly IEventBus _eventBus = EventBus.Default;
        private readonly CommandAndQueryHandler _handler = new();

        [Fact]
        public async Task Subscribe_WithoutSubscribe_ShouldQueryNothing()
        {
            var productsQuery = new ProductsQuery
            {
                Name = "Xiao"
            };

            await _eventBus.PublishAsync(this, productsQuery);

            Assert.Equal(null, productsQuery.Result);
        }

        [Fact]
        public async Task Subscribe_WithSubscribe_ShouldQuerySuccess()
        {
            var productsQuery = new ProductsQuery
            {
                Name = "Xiao"
            };

            _eventBus.Subscribe(_handler);
            await _eventBus.PublishAsync(this, productsQuery);
            Assert.Equal(null, productsQuery.Result);

            await _eventBus.PublishAsync(this, new CreateProductCommand() { Name = "XiaoMi", Price = 8999 });
            await _eventBus.PublishAsync(this, productsQuery);
            Assert.True(productsQuery.Result.Count > 0);
        }
    }
}