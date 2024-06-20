# CodeWF.EventBus

## 1. 前言

事件总线，即EventBus，是一种解耦模块间通讯的强大工具。在 [CodeWF.EventBus](https://github.com/dotnet9/CodeWF.EventBus) 中，我们得以轻松实现CQRS模式，并通过清晰、简洁的接口进行事件订阅与发布。接下来，我们将详细探讨如何使用这个库来处理事件。

> CQRS，全称Command Query Responsibility Segregation，是一种软件架构模式，旨在通过将系统中的命令（写操作）和查询（读操作）职责进行分离，来提高系统的性能、可伸缩性和响应性。

[CodeWF.EventBus](https://github.com/dotnet9/CodeWF.EventBus) 适用于进程内事件传递（无其他外部依赖），与 [MediatR](https://github.com/jbogard/MediatR) 功能类似。[MediatR](https://github.com/jbogard/MediatR)库侧重于[ASP.NET Core](https://learn.microsoft.com/zh-cn/aspnet/core/?view=aspnetcore-9.0)设计，且其功能更加强大，[CodeWF.EventBus](https://github.com/dotnet9/CodeWF.EventBus)库优势：

1. 小巧灵活，设计可在各种模板项目使用，如 WPF、Winform、Avalonia UI、ASP.NET Core 等。
2. 支持使用了任何 `IOC` 容器的项目。
3. 参考[MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)增强事件处理能力，支持一个类定义多个事件处理方法：

## 2. 使用说明

### 2.1. 注册事件总线

#### 2.1.1. MS.DI容器

主要是ASP.NET Core程序，比如 MVC、Razor Pages、Blazor Server 等模板程序，请搜索 NuGet 包`CodeWF.AspNetCore.EventBus`并安装最新版，安装完成后，在`Program`中添加如下代码：

```csharp
// ....

// 1、注册事件总线，将标注`EventHandler`特性方法的类采用单例方式注入IOC容器
builder.Services.AddEventBus();

var app = builder.Build();

// ...

// 2、将上面已经注入IOC容器的类取出、关联处理方法到事件总线管理
app.UseEventBus();

// ...
```

- `AddEventBus`方法会扫描传入的程序集列表，将标注`Event`特性的类下又标注`EventHandler`特性方法的类采用单例方式注入 IOC 容器。
- `UseEventBus`方法会将上一步注入的类通过 IOC 容器获取到实例，将实例的事件处理方法注册到事件管理队列中去，待收到事件发布时，会从事件管理队列中查找事件处理方法并调用，达到事件通知的功能。

#### 2.1.2. DryIOC容器

如果使用的`DryIoc`容器，比如 WPF /Avalonia UI中使用了 Prism 框架的DryIoc容器，请搜索 NuGet 包`CodeWF.DryIoc.EventBus`并安装最新版，安装完成后，在`RegisterTypes`方法中添加如下代码：

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    IContainer? container = containerRegistry.GetContainer();

    // ...

    // Register EventBus
    containerRegistry.AddEventBus();

    // ...

    // Use EventBus
    container.UseEventBus();
}
```

#### 2.1.3. 任意IOC容器

如果使用了其他`IOC`容器的项目，请搜索 NuGet 包`CodeWF.IOC.EventBus`并安装最新版，安装完成后，根据 IOC 容器注册单例、获取服务的 API 不同，做相应修改即可。

上面`ASP.NET Core`示例注册事件总线可改为：

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

支持任意`IOC`容器原理就在`AddEventBus`和`UseEventBus`方法：

```csharp
using CodeWF.EventBus;
using System.Reflection;

namespace CodeWF.IOC.EventBus
{
    public static class EventBusExtensions
    {
        public static void AddEventBus(Action<Type, Type> addSingleton1,
            Action<Type> addSingleton2, params Assembly[] assemblies)
        {
            addSingleton1(typeof(IEventBus), typeof(CodeWF.EventBus.EventBus));

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => addSingleton2(type),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);
        }

        public static void UseEventBus(Func<Type, object> resolveAction, params Assembly[] assemblies)
        {
            if (!(resolveAction(typeof(IEventBus)) is IEventBus messenger))
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(
                type => messenger.Subscribe(resolveAction(type)),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
        }
    }
}
```

```csharp
using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    public static class EventBusExtensions
    {
        public static void HandleEventObject(Action<Type> handleRecipient, BindingFlags findHandlerMethodBindingFlags,
            Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.GetCustomAttributes<EventAttribute>().Any()
                                && t.GetMethods(findHandlerMethodBindingFlags)
                                    .Any(m =>
                                        m.GetCustomAttributes<EventHandlerAttribute>().Any()));

                foreach (var type in types)
                {
                    handleRecipient(type);
                }
            }
        }
    }
}
```

#### 2.1.4. 未使用任何 IOC容器

默认的 WPF、Winform、Avalonia UI、控制台程序默认未引入任何 IOC 容器，这类项目我们可以不需要事件服务注册操作。

我们搜索 NuGet 包`CodeWF.EventBus`并安装最新版，安装完成后功能使用上和使用`IOC`容器一致，只是欠缺`IOC`注入自动订阅功能，具体差别请继续往下看。

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

在CQRS模式中，查询代表读操作。查询需要等待得到回应，适用于请求/响应。使用查询，调用方只需要关心我需要使用`ProductQuery`、`ProductsQuery`，而不必操心我需要`IProductService`、`ICategoryService`等服务获取查询结果。

定义查询类，继承自`Query<T>`:

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

`Query<T>`中T表示查询响应结果类型，在`XXXQuery`中使用`Result`属性表示查询发布后得到的结果。

`Query`继承自`Command`，带`Result`属性：

```csharp
public abstract class Query<TResult> : Command
{
    public abstract TResult Result { get; set; }
}
```

### 2.3. 订阅事件（Subscribe）

#### 2.3.1. 自动订阅

`自动订阅`只能在使用了`IOC`容器的程序中使用，比如`ASP.NET Core`程序。

一般将事件处理程序单独封装到一个类中，代码如下：

```csharp
[Event]
public class CommandAndQueryHandler(IEventBus eventBus, IProductService productService)
{
    [EventHandler]
    private async Task ReceiveCreateProductCommandAsync(CreateProductCommand command)
    {
        var isAddSuccess = await productService.AddProductAsync(new CreateProductRequest()
            { Name = command.Name, Price = command.Price });
        if (isAddSuccess)
        {
            await eventBus.PublishAsync(new CreateProductSuccessCommand()
                { Name = command.Name, Price = command.Price });
        }
        else
        {
            Console.WriteLine("Create product fail");
        }
    }

    [EventHandler(Order = 2)]
    private async Task ReceiveCreateProductSuccessCommandSendEmailAsync(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now send email notify create product success, name is = {command.Name}");
        await Task.CompletedTask;
    }

    [EventHandler(Order = 1)]
    private async Task ReceiveCreateProductSuccessCommandSendSmsAsync(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now send sms notify create product success, name is = {command.Name}");
        await Task.CompletedTask;
    }

    [EventHandler(Order = 3)]
    private void ReceiveCreateProductSuccessCommandCallPhone(CreateProductSuccessCommand command)
    {
        Console.WriteLine($"Now call phone notify create product success, name is = {command.Name}");
    }

    [EventHandler]
    private async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
        var isRemoveSuccess = await productService.RemoveProductAsync(command.ProductId);
        Console.WriteLine(isRemoveSuccess ? "Remote product success" : "Remote product fail");
    }

    [EventHandler]
    private async Task ReceiveProductQueryAsync(ProductQuery query)
    {
        var product = await productService.QueryProductAsync(query.ProductId);
        query.Result = product;
    }

    [EventHandler]
    private async Task ReceiveAutoProductsQueryAsync(ProductsQuery query)
    {
        var products = await productService.QueryProductsAsync(query.Name);
        query.Result = products;
    }

    [EventHandler]
    private static async Task ReceiveAutoProductsQueryAsync2(ProductsQuery query)
    {
        Console.WriteLine("Test auto subscribe static method");
    }
}
```

- 类`CommandAndQueryHandler`添加了`Event`特性，在 `IOC` 容器注入时标识为可以做为单例注入。
- 标注了`EventHandler`特性的方法拥有处理事件的能力，该方法只能有一个事件类型参数；如果方法支持异步，也只支持`Task`返回值，不能加泛型声明（加了无效）；支持静态事件处理方法。

使用 IOC 容器的程序会自动将标注`Event`特性的类做为单例注入容器，事件总线收到事件通知时自动查找标注`EventHandler`特性的方法进行调用，达到事件通知的功能。

#### 2.3.2. 手动订阅

对于未标注`Event`特性的类，可手动注册事件处理程序，如下是未使用 `IOC`容器时手动注册示例（核心是`EventBus.Default`使用）：

```csharp
internal class CommandAndQueryHandler
{
    internal void ManuSubscribe()
    {
        EventBus.Default.Subscribe<DeleteProductCommand>(ReceiveDeleteProductCommandAsync);
        EventBus.Default.Subscribe<ProductQuery>(ReceiveProductQueryAsync);
        EventBus.Default.Subscribe<ProductsQuery>(ReceiveAutoProductsQueryAsync2);
    }

    public async Task ReceiveDeleteProductCommandAsync(DeleteProductCommand command)
    {
    }

    public async Task ReceiveProductQueryAsync(ProductQuery query)
    {
    }

    private static async Task ReceiveAutoProductsQueryAsync2(ProductsQuery query)
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
    }

    // ...省略N多事件处理方法，EventBus.Default.Subscribe(this)方法可以自动绑定
}
```

使用了 `IOC`容器，可以注入`IEventBus`服务替换`EventBus.Default`使用，下如示例代码：

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

`EventBus`是`IEventBus`接口的默认实现，`EventBus.Default`是单例引用，所有两者使用任选其一。`IOC`注入时默认将`IEventBus`和`EventBus`做为单例注入，所以与两者等价。

手动订阅可以在 WPF 的 `XxxViewModel` 中使用（上面代码即是），也可以在 `IOC` 其他生命周期的服务中使用：

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

手动注册可运用在无法或不需要单例注入的情况使用，补充特殊情况。

### 2.4. 发布事件

发布命令(Command)与发布查询(Query)使用相同的接口，通过`IEventBus`或`EventBus.Default`的`Publish`和`PublishAsync`方法发布：

```csharp
_messenger.Publish(this, new DeleteProductCommand { ProductId = id });
```

```csharp
var query = new ProductQuery { ProductId = id };
await _messenger.PublishAsync(this, query);
Console.WriteLine($"查询产品ID为{id}的产品结果是：{query.Result}");
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
    public async Task AddAsync([FromBody] CreateProductRequest request)
    {
        await _eventBus.PublishAsync(new CreateProductCommand { Name = request.Name, Price = request.Price });
    }

    [HttpDelete("/delete")]
    public async Task DeleteAsync([FromQuery] Guid id)
    {
        await _eventBus.PublishAsync(new DeleteProductCommand { ProductId = id });
    }

    [HttpGet("/get")]
    public async Task<ProductItemDto> GetAsync([FromQuery] Guid id)
    {
        var query = new ProductQuery { ProductId = id };
        await _eventBus.PublishAsync(query);
        return query.Result;
    }

    [HttpGet("/list")]
    public async Task<List<ProductItemDto>> ListAsync([FromQuery] string? name)
    {
        var query = new ProductsQuery { Name = name };
        await _eventBus.PublishAsync(query);
        return query.Result;
    }
}
```

在`WPF/Avalonia UI`的`XXXViewModel`中使用:

```csharp
public class EventBusTestViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;

    public MessageTestViewModel(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task ExecuteEventBusAsync()
    {
        await _eventBus.PublishAsync(this, new TestMessage(nameof(MessageTestViewModel), TestClass.CurrentTime()));
    }
}
```

### 2.5. 取消订阅事件

在实际应用中，你可能需要确保在适当的时机（如服务销毁时）取消订阅，以避免内存泄漏：

1. 注销指定处理程序：`Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage)`
2. 注销指定类的所有处理程序：`Messenger.Default.Unsubscribe(this)`

## 3. 核心接口说明

```csharp
public interface IEventBus
{
    void Subscribe<T>() where T : class;
    void Subscribe(Type type);
    void Subscribe(object recipient);
    void Subscribe<TCommand>(Action<TCommand> action) where TCommand : Command;
    void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;
    void Unsubscribe<T>() where T : class;
    void Unsubscribe(object recipient);
    void Unsubscribe<TCommand>(Action<TCommand> action) where TCommand : Command;
    void Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;
    void Publish<TCommand>(TCommand command) where TCommand : Command;
    Task PublishAsync<TCommand>(TCommand command) where TCommand : Command;
}
```

- `Subscribe<T>()`：订阅类中静态事件处理方法
- `Subscribe(Type type)`：订阅指定类类型中静态事件处理方法
- `Subscribe(object recipient)`：订阅指定实例的成员事件处理方法
- `Subscribe<TCommand>(Action<TCommand> action)`：订阅普通事件处理方法，包括静态事件处理方法
- `Subscribe<TCommand>(Func<TCommand, Task> asyncAction)`：订阅异步事件处理方法，包括静态异步事件处理方法
- `Unsubscribe<T>()`：注销类中静态事件处理方法
- `Unsubscribe(object recipient)`：注销指定实例的成员事件处理方法
- `Unsubscribe<TCommand>(Action<TCommand> action)`：注销普通事件处理方法，包括静态事件处理方法
- `Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction)`：注销异步事件处理方法，包括静态异步事件处理方法
- `Publish<TCommand>(TCommand command)`：同步发布命令(Command)或查询(Query)
- `PublishAsync<TCommand>(TCommand command)`：异步发布命令(Command)或查询(Query)

## 4. 总结

`CodeWF.EventBus`提供了一个小巧灵活的事件总线实现，支持CQRS模式，并适用于各种项目模板，如 Avalonia UI、WPF、WinForms、ASP.NET Core 等。通过简单的订阅和发布操作，你可以轻松实现模块间的解耦和通讯。通过有序的事件处理，确保事件得到妥善处理。

事件总线具体实现请查看`CodeWF.EventBus`源码： https://github.com/dotnet9/CodeWF.EventBus ，具体使用可参考：

1. 单元测试：[CodeWF.EventBus.Tests](https://github.com/dotnet9/CodeWF.EventBus/tree/main/src/CodeWF.EventBus.Tests)
3. AvaloniaUI + Prism：[Tools.CodeWF](https://github.com/dotnet9/Tools.CodeWF/tree/prism-modules)
4. Web API：[WebAPIDemo](https://github.com/dotnet9/CodeWF.EventBus/tree/main/src/WebAPIDemo) 、[CodeWF](https://github.com/dotnet9/CodeWF)

开发参考开源项目：

1. [Messenger | MvvmCross](https://www.mvvmcross.com/documentation/plugins/messenger?scroll=1000)
2. [Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)
3. [MediatR](https://github.com/jbogard/MediatR)
4. [MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)

希望本文的指南能帮助你更好地使用`CodeWF.EventBus`来处理你的应用程序中的事件。
