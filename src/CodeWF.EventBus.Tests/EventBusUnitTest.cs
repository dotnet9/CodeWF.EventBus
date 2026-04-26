using CodeWF.EventBus.Tests.Commands;
using CodeWF.EventBus.Tests.Handlers;
using CodeWF.EventBus.Tests.Queries;
using CommandAndQueryModel.Commands;
using CommandAndQueryModel.Queries;
using CommandAndQueryModel.Services;

namespace CodeWF.EventBus.Tests
{
    public class EventBusUnitTest
    {
        private readonly IEventBus _eventBus;
        private readonly CommandAndQueryHandler _handler;

        public EventBusUnitTest()
        {
            ProductService.Reset();
            StaticHandler.TestCount = 0;
            CommandAndQueryHandler.ResetTestState();

            _eventBus = new EventBus();
            _handler = new CommandAndQueryHandler(_eventBus, ProductService.Default);
        }

        [Fact]
        public async Task Subscribe_WithoutSubscribe_ShouldQueryNothing()
        {
            var productsQuery = new ProductsQuery
            {
                Name = "Xiao"
            };

            await _eventBus.PublishAsync(productsQuery);

            Assert.Null(productsQuery.Result);
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
            Assert.NotNull(productsQuery.Result);
            Assert.Empty(productsQuery.Result);

            await _eventBus.PublishAsync(new CreateProductCommand() { Name = "XiaoMi", Price = 8999 });
            await _eventBus.PublishAsync(productsQuery);
            Assert.NotNull(productsQuery.Result);
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
        public async Task Should_NotDuplicateSameStaticSubscription()
        {
            var query = new TestQuery();

            _eventBus.Subscribe<TestAddCommand>(CommandAndQueryHandler.ReceiveAddCommand);
            _eventBus.Subscribe<TestAddCommand>(CommandAndQueryHandler.ReceiveAddCommand);
            _eventBus.Subscribe<TestQuery>(CommandAndQueryHandler.ReceiveStaticQuery);

            await _eventBus.PublishAsync(new TestAddCommand());
            var addCount = await _eventBus.QueryAsync(query);

            Assert.Equal(1, addCount);
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

        [Fact]
        public async Task Publish_WithAutoHandlersWithoutServiceResolver_ShouldThrowHelpfulException()
        {
            var eventBus = new EventBus();
            eventBus.Subscribe(new[] { typeof(AutoResolverHandler).Assembly });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                eventBus.PublishAsync(new AutoResolverCommand()));

            Assert.Contains("RegisterServiceHandlerAction", exception.Message);
        }

        [Event]
        private sealed class AutoResolverHandler
        {
            [EventHandler]
            private void Handle(AutoResolverCommand command)
            {
            }
        }

        private sealed class AutoResolverCommand : Command
        {
        }
    }
}
