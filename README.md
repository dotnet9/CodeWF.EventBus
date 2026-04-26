# CodeWF.EventBus

| Name | NuGet | Download |
|------|-----------|--------|
| CodeWF.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.EventBus)](https://www.nuget.org/packages/CodeWF.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.EventBus)](https://www.nuget.org/packages/CodeWF.EventBus/) |
| CodeWF.IOC.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.IOC.EventBus.svg)](https://www.nuget.org/packages/CodeWF.IOC.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.IOC.EventBus.svg)](https://www.nuget.org/packages/CodeWF.IOC.EventBus/) |
| CodeWF.DryIoc.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.DryIoc.EventBus.svg)](https://www.nuget.org/packages/CodeWF.DryIoc.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.DryIoc.EventBus.svg)](https://www.nuget.org/packages/CodeWF.DryIoc.EventBus/) |
| CodeWF.AspNetCore.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.AspNetCore.EventBus.svg)](https://www.nuget.org/packages/CodeWF.AspNetCore.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.AspNetCore.EventBus.svg)](https://www.nuget.org/packages/CodeWF.AspNetCore.EventBus/) |

## 简介

`CodeWF.EventBus` 是一个轻量的进程内事件总线库，适合在 WPF、WinForms、Avalonia UI、ASP.NET Core 和控制台程序中做模块解耦。

它支持两类典型场景：

1. `Command` 命令分发。
2. `Query<T>` 查询回传，方便实现简单 CQRS。

如果你熟悉 `MediatR`、`Prism.Events` 或 `MASA Framework` 的事件处理方式，可以把它理解成一个更轻量、对项目类型约束更少的选择。

设计说明可查看：

- [docs/CodeWF.EventBus-Design.md](./docs/CodeWF.EventBus-Design.md)

## 安装

按项目类型选择包：

- 无 IOC 容器：`CodeWF.EventBus`
- ASP.NET Core / MS.DI：`CodeWF.AspNetCore.EventBus`
- DryIoc / Prism：`CodeWF.DryIoc.EventBus`
- 其他 IOC 容器：`CodeWF.IOC.EventBus`

## 核心类型

```csharp
public abstract class Command
{
}

public abstract class Query<TResponse> : Command
{
    public abstract TResponse Result { get; set; }
}
```

示例：

```csharp
public sealed class CreateProductCommand : Command
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public sealed class ProductQuery : Query<ProductItemDto?>
{
    public Guid ProductId { get; set; }
    public override ProductItemDto? Result { get; set; }
}
```

## 处理器写法

事件处理方法使用 `[EventHandler]` 标记，参数只能有一个，且必须继承自 `Command`。返回值只支持：

- `void`
- `Task`

方法声明支持：

- `public`
- `private`
- `static`

说明：

- `Subscribe<T>()` / `Subscribe(Type)` 会扫描指定类型中的 `public/private static` 处理方法。
- `Subscribe(this)` 会扫描当前实例中的 `public/private instance` 处理方法。
- `Subscribe(Assembly[])` 会登记标记了 `[Event]` 的类型中 `public/private instance` 处理方法，真正执行时再通过服务解析器拿实例。

示例：

```csharp
[Event]
public sealed class ProductEventHandler
{
    [EventHandler]
    private async Task HandleCreateAsync(CreateProductCommand command)
    {
        await Task.CompletedTask;
    }

    [EventHandler]
    private void HandleQuery(ProductQuery query)
    {
        query.Result = new ProductItemDto
        {
            Id = query.ProductId,
            Name = "Demo",
            Price = 99
        };
    }
}
```

`[Event]` 主要用于 IOC 自动发现实例处理器。通过 `Subscribe<T>()` 这类方式扫描指定类型时，不需要再额外标记 `[Event]`：

```csharp
public static class TimeHandler
{
    [EventHandler]
    private static void Handle(UpdateTimeCommand command)
    {
        Console.WriteLine(command.Time);
    }
}
```

## 无 IOC 容器

WPF、WinForms、Avalonia UI、控制台等未接入 IOC 时，建议直接使用 `EventBus.Default` 或自己 new 一个 `EventBus`。

### 手动注册实例处理器

```csharp
public sealed class MainViewModel
{
    private readonly IEventBus _eventBus;

    public MainViewModel()
    {
        _eventBus = EventBus.Default;
        _eventBus.Subscribe(this);
    }

    [EventHandler]
    private void Handle(UpdateTimeCommand command)
    {
        Console.WriteLine(command.Time);
    }
}
```

### 手动注册类型处理器

```csharp
var eventBus = EventBus.Default;

eventBus.Subscribe<TimeHandler>();
eventBus.Publish(new UpdateTimeCommand("2026-04-26 10:00:00"));
```

### 发布命令和查询

```csharp
await EventBus.Default.PublishAsync(new CreateProductCommand
{
    Name = "XiaoMi",
    Price = 8999
});

var product = await EventBus.Default.QueryAsync(new ProductQuery
{
    ProductId = Guid.NewGuid()
});
```

### 取消订阅

实例对象不再使用时，建议主动取消订阅：

```csharp
EventBus.Default.Unsubscribe(this);
```

通过扫描指定类型注册的处理器也可以取消：

```csharp
EventBus.Default.Unsubscribe<TimeHandler>();
```

## ASP.NET Core / MS.DI

安装 `CodeWF.AspNetCore.EventBus` 后，在 `Program.cs` 中注册：

```csharp
using CodeWF.AspNetCore.EventBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddEventBus();

var app = builder.Build();

app.MapControllers();
app.UseEventBus();

app.Run();
```

说明：

- `AddEventBus()` 会扫描程序集中的 `[Event]` 类，并将它们按作用域注册到容器中。
- `UseEventBus()` 会把扫描指定类型得到的处理器和实例处理器都接入事件总线。

控制器中直接注入 `IEventBus`：

```csharp
[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventBus _eventBus;

    public EventController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    [HttpPost("/add")]
    public async Task AddAsync([FromBody] CreateProductRequest request)
    {
        await _eventBus.PublishAsync(new CreateProductCommand
        {
            Name = request.Name,
            Price = request.Price
        });
    }

    [HttpGet("/get")]
    public async Task<ActionResult<ProductItemDto>> GetAsync([FromQuery] Guid id)
    {
        var product = await _eventBus.QueryAsync(new ProductQuery { ProductId = id });
        return product == null ? NotFound() : Ok(product);
    }
}
```

## DryIoc / Prism

安装 `CodeWF.DryIoc.EventBus` 后：

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    var container = containerRegistry.GetContainer();

    containerRegistry.AddEventBus();
    container.UseEventBus();
}
```

## 其他 IOC 容器

安装 `CodeWF.IOC.EventBus` 后，把你的“注册单例 / 注册作用域 / 按类型解析”能力传进去：

```csharp
using CodeWF.IOC.EventBus;

EventBusExtensions.AddEventBus(
    (serviceType, implementationType) => builder.Services.AddSingleton(serviceType, implementationType),
    serviceType => builder.Services.AddScoped(serviceType),
    Assembly.GetExecutingAssembly());

var app = builder.Build();

EventBusExtensions.UseEventBus(
    serviceType => app.Services.GetRequiredService(serviceType),
    Assembly.GetExecutingAssembly());
```

## 自动发现与注意事项

### 1. 自动发现实例处理器依赖服务解析器

`Subscribe(Assembly[])` 用于登记程序集中的 `[Event]` 实例处理器，但真正执行这些处理器时需要先有服务解析器。

所以通常应优先使用：

- `app.UseEventBus()`
- `container.UseEventBus()`
- `EventBusExtensions.UseEventBus(...)`

如果没有 IOC 容器，请使用：

- `Subscribe(this)` 注册实例对象
- `Subscribe<T>()` 扫描指定类型并注册处理器

### 2. 重复订阅会自动去重

同一个对象方法或同一个扫描命中的方法重复注册，不会重复执行。

### 3. 查询结果的约定

`Query<T>` 的结果由处理器写入 `Result`。是否允许返回 `null` 由你的查询类型决定，例如：

```csharp
public sealed class ProductQuery : Query<ProductItemDto?>
{
    public override ProductItemDto? Result { get; set; }
}
```

### 4. 执行顺序

`[EventHandler(Order = n)]` 可以控制同一命令下多个处理器的执行顺序，值越小越先执行。

## 当前接口

```csharp
public interface IEventBus
{
    void Subscribe<T>() where T : class;
    void Subscribe(Type type);
    void Subscribe(object recipient);
    void Subscribe<TCommand>(Action<TCommand> action) where TCommand : Command;
    void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;
    void Subscribe(Assembly[] assemblies);

    void Unsubscribe<T>() where T : class;
    void Unsubscribe(object recipient);
    void Unsubscribe<TCommand>(Action<TCommand> action) where TCommand : Command;
    void Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;

    void Publish<TCommand>(TCommand command) where TCommand : Command;
    TResponse Query<TResponse>(Query<TResponse> query);
    Task PublishAsync<TCommand>(TCommand command) where TCommand : Command;
    Task<TResponse> QueryAsync<TResponse>(Query<TResponse> query);

    void RegisterServiceHandlerAction(Action<Type, Action<object>> serviceHandlerAction);
}
```

## 示例项目

仓库内可直接参考：

- [单元测试](./src/CodeWF.EventBus.Tests/)
- [WebAPIDemo](./src/WebAPIDemo/)
- [ConsoleAOT](./src/CodeWF.EventBus.ConsoleAOT/)
- [WindowsFormsApp1_4_8](./src/WindowsFormsApp1_4_8/)
- [Avalonia AOT Demo](./src/CodeWF.EventBus.AvaAOT/)

## 参考

1. [MediatR](https://github.com/jbogard/MediatR)
2. [Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)
3. [Messenger | MvvmCross](https://www.mvvmcross.com/documentation/plugins/messenger?scroll=1000)
4. [MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)
