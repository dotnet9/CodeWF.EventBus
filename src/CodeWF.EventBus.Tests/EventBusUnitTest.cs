using CodeWF.EventBus.Tests.Commands;
using CodeWF.EventBus.Tests.Handlers;
using CodeWF.EventBus.Tests.Queries;
using CommandAndQueryModel.Commands;
using CommandAndQueryModel.Queries;
using CommandAndQueryModel.Services;
using System.Reflection;

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

        [Fact]
        public async Task Publish_ShouldRespectOrderAcrossManualAndAutoHandlers()
        {
            var eventBus = new EventBus();
            var executionOrder = new List<string>();

            eventBus.Subscribe<OrderCommand>(_ => executionOrder.Add("manual"));
            eventBus.RegisterServiceHandlerAction((type, action) =>
            {
                action(new OrderedAutoHandler(value => executionOrder.Add(value)));
            });
            eventBus.Subscribe(new[] { typeof(OrderedAutoHandler).Assembly });

            await eventBus.PublishAsync(new OrderCommand());

            Assert.Equal(new[] { "auto", "manual" }, executionOrder);
        }

        [Fact]
        public async Task Publish_ShouldIgnoreAutoHandlersWithInvalidReturnType()
        {
            var eventBus = new EventBus();
            eventBus.RegisterServiceHandlerAction((type, action) =>
            {
                action(new InvalidReturnHandler());
            });
            eventBus.Subscribe(new[] { typeof(InvalidReturnHandler).Assembly });

            await eventBus.PublishAsync(new InvalidReturnCommand());
        }

        [Fact]
        public async Task Subscribe_ShouldRejectNullDelegates()
        {
            var eventBus = new EventBus();

            Assert.Throws<ArgumentNullException>(() =>
                eventBus.Subscribe<TestAddCommand>((Action<TestAddCommand>)null!));
            Assert.Throws<ArgumentNullException>(() =>
                eventBus.Subscribe<TestAddCommand>((Func<TestAddCommand, Task>)null!));

            eventBus.Subscribe<TestAddCommand>(_ => null!);
            await eventBus.PublishAsync(new TestAddCommand());
        }

        [Fact]
        public void Publish_ShouldRejectAsyncHandlers()
        {
            var eventBus = new EventBus();
            var invoked = false;
            eventBus.Subscribe<TestAddCommand>((Func<TestAddCommand, Task>)(async _ =>
            {
                invoked = true;
                await Task.Yield();
            }));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                eventBus.Publish(new TestAddCommand()));

            Assert.Contains("PublishAsync", exception.Message);
            Assert.False(invoked);
        }

        [Fact]
        public async Task PublishAsync_ShouldPropagateOriginalHandlerException()
        {
            var eventBus = new EventBus();
            var expected = new InvalidOperationException("handler failed");
            eventBus.Subscribe<TestAddCommand>(_ => throw expected);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                eventBus.PublishAsync(new TestAddCommand()));

            Assert.Same(expected, exception);
        }

        [Fact]
        public void HandleEventObject_ShouldValidateArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EventBusExtensions.HandleEventObject(null!, BindingFlags.Instance, Array.Empty<Assembly>()));
            Assert.Throws<ArgumentNullException>(() =>
                EventBusExtensions.HandleEventObject(_ => { }, BindingFlags.Instance, null!));
        }

        [Fact]
        public async Task Publish_ShouldRejectNullResolvedAutoHandler()
        {
            var eventBus = new EventBus();
            eventBus.RegisterServiceHandlerAction((type, action) => action(null!));
            eventBus.Subscribe(new[] { typeof(AutoResolverHandler).Assembly });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                eventBus.PublishAsync(new AutoResolverCommand()));

            Assert.Contains("returned null", exception.Message);
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

        private sealed class OrderCommand : Command
        {
        }

        private sealed class InvalidReturnCommand : Command
        {
        }

        [Event]
        private sealed class OrderedAutoHandler
        {
            private readonly Action<string> _record;

            public OrderedAutoHandler(Action<string> record)
            {
                _record = record;
            }

            [EventHandler(Order = -1)]
            private void Handle(OrderCommand command)
            {
                _record("auto");
            }
        }

        [Event]
        private sealed class InvalidReturnHandler
        {
            [EventHandler]
            private int Handle(InvalidReturnCommand command)
            {
                return 1;
            }
        }
    }
}
