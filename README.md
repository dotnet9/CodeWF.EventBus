# CodeWF.EventBus

## 1. 简介

事件总线，即EventBus，是一种解耦模块间通讯的强大工具。在[CodeWF.EventBus](https://www.nuget.org/packages?q=CodeWF.EventBus)库中，我们得以轻松实现CQRS模式，并通过清晰、简洁的接口进行事件订阅与发布。接下来，我们将详细探讨如何使用这个库来处理事件。

> CQRS，全称Command Query Responsibility Segregation，是一种软件架构模式，旨在通过将系统中的命令（写操作）和查询（读操作）职责进行分离，来提高系统的性能、可伸缩性和响应性。

[CodeWF.EventBus](https://www.nuget.org/packages?q=CodeWF.EventBus)适用于进程内事件传递（无其他外部依赖），与[MediatR](https://github.com/jbogard/MediatR)功能类似。[MediatR](https://github.com/jbogard/MediatR)库侧重于[ASP.NET Core](https://learn.microsoft.com/zh-cn/aspnet/core/?view=aspnetcore-9.0)设计，且其功能更加强大，[CodeWF.EventBus](https://www.nuget.org/packages?q=CodeWF.EventBus)库优势：

1. 小巧灵活，设计可在各种模板项目使用，如 WPF、Winform、Avalonia UI、ASP.NET Core 等。
2. 支持使用了任何 IOC 容器的项目，当然也支持未使用任何 IOC 容器的模板项目。
3. 参考[MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)增强事件处理能力，支持一个类定义多个事件处理方法：

## 2. 怎么使用事件总线？

首先请搜索 NuGet 包`CodeWF.EventBus`并安装最新版，安装完成后，你就可以在你的代码中引用并使用`CodeWF.EventBus`了。

### 2.1. 注册事件总线

#### 2.1.1. 使用了 IOC

如果是 ASP.NET Core 程序，比如 MVC、Razor Pages、Blazor Server 等模板程序，在`Program`中添加如下代码：

```csharp
// ....

// 1、注册事件总线，将标注`EventHandler`特性方法的类采用单例方式注入IOC容器
EventBusExtensions.AddEventBus(
    (t1, t2) => builder.Services.AddSingleton(t1, t2),
    t => builder.Services.AddSingleton(t),
    Assembly.GetExecutingAssembly());

var app = builder.Build();

// ...

// 2、将上面已经注入IOC容器的类取出、关联处理方法到事件总线管理
EventBusExtensions.UseEventBus(t => app.Services.GetRequiredService(t), Assembly.GetExecutingAssembly());

// ...
```

- `AddEventBus`方法会扫描传入的程序集列表，将标注`Event`特性的类下又标注`EventHandler`特性方法的类采用单例方式注入 IOC 容器。
- `UseEventBus`方法会将上一步注入的类通过 IOC 容器获取到实例，将实例的事件处理方法注册到事件管理队列中去，待收到事件发布时，会从事件管理队列中查找事件处理方法并调用，达到事件通知的功能。

如果使用的其他 IOC容器，比如 WPF 中使用了 Prism 框架的DryIoc容器，写法如下：

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    IContainer? container = containerRegistry.GetContainer();

    // ...

    // Register EventBus
    EventBusExtensions.AddEventBus(
        (t1,t2)=> containerRegistry.RegisterSingleton(t1,t2),
        t=> containerRegistry.RegisterSingleton(t),
        typeof(App).Assembly);

    // ...

    // Use EventBus
    EventBusExtensions.UseEventBus(t => container.Resolve(t), typeof(App).Assembly);
}
```

根据 IOC 容器注册单例、获取服务的 API 不同，做相应修改即可。

#### 2.1.2. 未使用 IOC

默认的 WPF、Winform、AvaloniaUI、控制台程序默认未引入任何 IOC 容器，这里不用做事件服务注册操作，功能使用上和使用IOC只差自动订阅功能，其他功能一样。

### 2.2. 定义事件

在这里我们使用 CQRS 来完成我们程序业务逻辑，在 CQRS 模式中我们的查询和其它业务操作是分开的。不了解 CQRS 的可以看看这篇文章：https://learn.microsoft.com/zh-cn/azure/architecture/patterns/cqrs

#### 2.2.1. 定义命令(Command)

在CQRS模式中，命令代表写操作。定义命令类，这些类继承自`Command`类

```CSharp
public class CreateProductCommand : Command
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
public class CreateProductSuccessCommand : Command
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
public class DeleteProductCommand : Command
{
    public Guid ProductId { get; set; }
}
```

#### 2.2.2. 定义查询(Query)

在CQRS模式中，查询代表读操作。查询需要等待得到回应，适用于请求/响应。使用查询，调用方只需要关心我需要使用XXQuery，而不必操心我需要XXXService、AAService。定义查询类，继承自`Query<T>`:

```csharp
public class ProductQuery : Query<ProductItemDto>
{
    public Guid ProductId { get; set; }
    public override ProductItemDto Result { get; set; }
}
public class ProductsQuery : Query<List<ProductItemDto>>
{
    public string Name { get; set; }
    public override List<ProductItemDto> Result { get; set; }
}
```

`Query<T>`中T表示查询响应结果类型，在查房中使用`Result`属性表示。

### 2.3. 订阅事件（事件）

#### 2.3.1. 自动订阅

在`B/S`程序中，一般将事件处理程序单独封装到一个类中，文章开头贴的代码中`CommandAndQueryHandler`即是自动订阅类格式，这里我们再贴上讲解：

```csharp
[Event]
public class CommandAndQueryHandler(IEventBus eventBus, IProductService productService)
{
    [EventHandler]
    public async Task ReceiveCreateProductCommandAsync(CreateProductCommand command)
    {
        var isAddSuccess = await productService.AddProductAsync(new CreateProductRequest()
            { Name = command.Name, Price = command.Price });
        if (isAddSuccess)
        {
            await eventBus.PublishAsync(this,
                new CreateProductSuccessCommand() { Name = command.Name, Price = command.Price });
        }
        else
        {
            Console.WriteLine("Create product fail");
        }
    }

    [EventHandler(Order = 2)]
    public async Task ReceiveCreateProductSuccessCommandSendEmailAsync(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now send email notify create product success, name is = {command.Name}");
        await Task.CompletedTask;
    }

    [EventHandler(Order = 1)]
    public async Task ReceiveCreateProductSuccessCommandSendSmsAsync(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now send sms notify create product success, name is = {command.Name}");
        await Task.CompletedTask;
    }

    [EventHandler(Order = 3)]
    public void ReceiveCreateProductSuccessCommandCallPhone(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now call phone notify create product success, name is = {command.Name}");
    }

    [EventHandler]
    public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
        var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
        Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
    }

    [EventHandler]
    public async Task ReceiveProductQueryAsync(ProductQuery query)
    {
        var product = await productService.QueryProductAsync(query.ProductId);
        query.Result = product;
    }

    [EventHandler]
    public async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
    {
        var products = await productService.QueryProductsAsync(query.Name);
        query.Result = products;
    }
}
```

- 类`CommandAndQueryHandler`添加了`Event`特性，在 IOC 容器注入时标识为可以做为单例注入。
- 标注了`EventHandler`特性的方法拥有处理事件的能力，该方法只能有一个事件类型参数；如果方法支持异步，也只支持`Task`返回值，不能加泛型声明（加了无效）。

使用 IOC 容器的程序会自动将标注`Event`特性的类做为单例注入容器，事件总线收到事件通知时自动查找标注`EventHandle`特性的方法进行调用，达到事件通知的功能。

#### 2.3.2. 手动订阅

对于未标注`Event`特性的类，可手动注册事件处理程序，如下是未使用 IOC容器时手动注册示例（核心是`EventBus.Default`使用）：

```csharp
internal class CommandAndQueryHandler
{
    internal void ManuSubscribe()
    {
        EventBus.Default.Subscribe<DeleteProductCommand>(this, ReceiveDeleteProductCommandAsync);
        EventBus.Default.Subscribe<ProductQuery>(this, ReceiveProductQueryAsync);
    }

    public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
    }

    public async Task ReceiveProductQueryAsync(ProductQuery query)
    {
    }
}
```

上面挨个注册处理方法有时会过于啰嗦，可以简化：

```csharp
internal class CommandAndQueryHandler
{
    internal CommandAndQueryHandler()
    {
        EventBus.Default.Subscribe(this);
    }

    [EventHandler(Order = 2)]
    public async Task ReceiveCreateProductSuccessCommandSendEmailAsync(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now send email notify create product success, name is = {command.Name}");
        await Task.CompletedTask;
    }

    [EventHandler(Order = 1)]
    public async Task ReceiveCreateProductSuccessCommandSendSmsAsync(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now send sms notify create product success, name is = {command.Name}");
        await Task.CompletedTask;
    }

    [EventHandler(Order = 3)]
    public void ReceiveCreateProductSuccessCommandCallPhone(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now call phone notify create product success, name is = {command.Name}");
    }

    [EventHandler]
    public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
        var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
        Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
    }

    [EventHandler]
    public async Task ReceiveProductQueryAsync(ProductQuery query)
    {
        var product = await productService.QueryProductAsync(query.ProductId);
        query.Result = product;
    }

    [EventHandler]
    public async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
    {
        var products = await productService.QueryProductsAsync(query.Name);
        query.Result = products;
    }
}
```

使用了 IOC容器，可以注入`IEventBus`服务替换`EventBus.Default`使用，`EventBus`是`IEventBus`接口的默认实现，`EventBus.Default`是单例引用。

```csharp
public class EventBusTestViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;

    public MessageTestViewModel(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe(this);
    }
    
    [EventHandler]
    public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
        var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
        Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
    }
}
```

手动订阅可以在 WPF 的 ViewModel 中使用（代码如上），也可以在 IOC 其他生命周期的服务中使用：

```csharp
public class TimeService : ITimeService
{
    private readonly IEventBus _eventBus;

    public TimeService(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe(this);
    }
    
	[EventHandler]
    public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
        var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
        Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
    }
}
```

手动注册可运用在无法或不需要单例注入的情况使用。

### 2.4. 发布事件

发布命令与查询使用相同的接口，通过`IEventBus`或`EventBus.Default`的`Publish`和`PublishAsync`方法发布命令和查询：

```csharp
_messenger.Publish(this, new DeleteProductCommand { ProductId = id });
```

```csharp
var query = new ProductQuery { ProductId = id };
await _messenger.PublishAsync(this, query);
```

在`B/S`控制器的`Action`使用发布：

```csharp
[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;
    private readonly IEventBus _eventBus;

    public EventController(ILogger<EventController> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    [HttpPost("/add")]
    public Task AddAsync([FromBody] CreateProductRequest request)
    {
        _eventBus.Publish(this, new CreateProductCommand { Name = request.Name, Price = request.Price });
        return Task.CompletedTask;
    }

    [HttpDelete("/delete")]
    public Task DeleteAsync([FromQuery] Guid id)
    {
        _eventBus.Publish(this, new DeleteProductCommand { ProductId = id });
        return Task.CompletedTask;
    }

    [HttpGet("/get")]
    public async Task<ProductItemDto> GetAsync([FromQuery] Guid id)
    {
        var query = new ProductQuery { ProductId = id };
        await _eventBus.PublishAsync(this, query);
        return query.Result;
    }

    [HttpGet("/list")]
    public async Task<List<ProductItemDto>> ListAsync([FromQuery] string? name)
    {
        var query = new ProductsQuery { Name = name };
        await _eventBus.PublishAsync(this, query);
        return query.Result;
    }
}
```

在`WPF/Avalonia UI`的`ViewModel`中使用:

```csharp
public class EventBusTestViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;

    public MessageTestViewModel(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task ExecuteEventBusAsync()
    {
        _eventBus.PublishAsync(this, new TestMessage(nameof(MessageTestViewModel), TestClass.CurrentTime()));
        return Task.CompletedTask;
    }
}
```

### 2.5. 取消订阅事件（事件）

在实际应用中，你可能需要确保在适当的时机（如服务销毁时）取消订阅，以避免内存泄漏：

1. 注销指定处理程序：`Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage)`
2. 注销指定类的所有处理程序：`Messenger.Default.Unsubscribe(this)`

## 3. 总结

CodeWF.EventBus提供了一个小巧灵活的事件总线实现，支持CQRS模式，并适用于各种项目模板，如 Avalonia UI、WPF、WinForms、ASP.NET Core 等。通过简单的订阅和发布操作，你可以轻松实现模块间的解耦和通讯。通过有序的事件处理，确保事件得到妥善处理。

仓库地址 https://github.com/dotnet9/CodeWF.EventBus，具体使用可参考：

1. 单元测试：[CodeWF.EventBus.Tests](https://github.com/dotnet9/CodeWF.EventBus/tree/main/src/CodeWF.EventBus.Tests)
3. AvaloniaUI + Prism：[Tools.CodeWF](https://github.com/dotnet9/Tools.CodeWF/tree/prism-modules)
4. Web API：[WebAPIDemo](https://github.com/dotnet9/CodeWF.EventBus/tree/main/src/WebAPIDemo)

开发过程中参考不少开源项目：

1. [Messenger | MvvmCross](https://www.mvvmcross.com/documentation/plugins/messenger?scroll=1000)
2. [Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)
3. [MediatR](https://github.com/jbogard/MediatR)
4. [MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)

希望本文的指南能帮助你更好地使用CodeWF.EventBus来处理你的应用程序中的事件。