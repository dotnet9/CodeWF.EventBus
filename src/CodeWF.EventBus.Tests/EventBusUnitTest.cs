using CodeWF.EventBus.Tests.Commands;
using CodeWF.EventBus.Tests.Handlers;
using CodeWF.EventBus.Tests.Queries;
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

            await _eventBus.PublishAsync(productsQuery);

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
            await _eventBus.PublishAsync(productsQuery);
            Assert.Equal(null, productsQuery.Result);

            await _eventBus.PublishAsync(new CreateProductCommand() { Name = "XiaoMi", Price = 8999 });
            await _eventBus.PublishAsync(productsQuery);
            Assert.True(productsQuery.Result.Count > 0);
        }

        [Fact]
        public async Task Should_SubscribeStaticHandle_Success()
        {
            var query = new TestQuery();
            Assert.Equal(0, query.Result);

            _eventBus.Subscribe<TestAddCommand>(CommandAndQueryHandler.ReceiveAddCommand);
            _eventBus.Subscribe<TestQuery>(CommandAndQueryHandler.ReceiveStaticQuery);

            await _eventBus.PublishAsync(new TestAddCommand());
            await _eventBus.PublishAsync(query);
            Assert.True(query.Result == 1);

            _eventBus.Unsubscribe<TestAddCommand>(CommandAndQueryHandler.ReceiveAddCommand);
            await _eventBus.PublishAsync(new TestAddCommand());
            await _eventBus.PublishAsync(query);
            Assert.True(query.Result == 1);

            _eventBus.Subscribe<TestAddCommand>(CommandAndQueryHandler.ReceiveAddCommand);
            await _eventBus.PublishAsync(new TestAddCommand());
            await _eventBus.PublishAsync(query);
            Assert.True(query.Result == 2);
        }

        [Fact]
        public async Task Should_AutoSubscribeStaticHandle_Success()
        {
            var query = new TestQuery();
            Assert.Equal(0, query.Result);

            _eventBus.Subscribe<StaticHandler>();

            await _eventBus.PublishAsync(new TestAddCommand());
            await _eventBus.PublishAsync(query);
            Assert.True(query.Result == 1);

            _eventBus.Unsubscribe<StaticHandler>();
            await _eventBus.PublishAsync(new TestAddCommand());
            await _eventBus.PublishAsync(query);
            Assert.True(query.Result == 1);

            _eventBus.Subscribe<StaticHandler>();
            await _eventBus.PublishAsync(new TestAddCommand());
            await _eventBus.PublishAsync(query);
            Assert.True(query.Result == 2);
        }

        [Fact]
        public async Task Should_UnsubscribeStaticHandle_Success()
        {
            var query = new TestQuery();
            var addCount = 0;
            Assert.Equal(0, query.Result);

            _eventBus.Subscribe<StaticHandler>();
            _eventBus.Subscribe<StaticHandler2>();

            await _eventBus.PublishAsync(new TestAddCommand());
            addCount = await _eventBus.QueryAsync(query);
            Assert.True(addCount == 2);

            _eventBus.Unsubscribe<StaticHandler>();
            await _eventBus.PublishAsync(new TestAddCommand());
            addCount = await _eventBus.QueryAsync(query);
            Assert.True(query.Result == 3);

            _eventBus.Unsubscribe<StaticHandler2>();
            await _eventBus.PublishAsync(new TestAddCommand());
            addCount = await _eventBus.QueryAsync(query);
            Assert.True(addCount == 3);

            _eventBus.Subscribe<StaticHandler2>();
            await _eventBus.PublishAsync(new TestAddCommand());
            addCount = await _eventBus.QueryAsync(query);
            Assert.True(addCount == 4);
        }
    }
}