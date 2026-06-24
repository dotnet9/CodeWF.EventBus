# CodeWF.EventBus

| 名称 | NuGet | 下载量 |
|------|-----------|--------|
| CodeWF.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.EventBus)](https://www.nuget.org/packages/CodeWF.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.EventBus)](https://www.nuget.org/packages/CodeWF.EventBus/) |
| CodeWF.IOC.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.IOC.EventBus.svg)](https://www.nuget.org/packages/CodeWF.IOC.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.IOC.EventBus.svg)](https://www.nuget.org/packages/CodeWF.IOC.EventBus/) |
| CodeWF.DryIoc.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.DryIoc.EventBus.svg)](https://www.nuget.org/packages/CodeWF.DryIoc.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.DryIoc.EventBus.svg)](https://www.nuget.org/packages/CodeWF.DryIoc.EventBus/) |
| CodeWF.AspNetCore.EventBus | [![NuGet](https://img.shields.io/nuget/v/CodeWF.AspNetCore.EventBus.svg)](https://www.nuget.org/packages/CodeWF.AspNetCore.EventBus/) | [![NuGet](https://img.shields.io/nuget/dt/CodeWF.AspNetCore.EventBus.svg)](https://www.nuget.org/packages/CodeWF.AspNetCore.EventBus/) |

## 仓库规范

- 当前版本：`3.4.5.8`，版本号统一维护在根目录 `Directory.Build.props` 的 `<Version>` 节点。
- NuGet 包项目统一支持 `net8.0;net10.0`；Demo、App、测试与内部应用项目统一使用 `net11.0` / `net11.0-windows`。
- 根目录 `logo.svg`、`logo.png`、`logo.ico` 是唯一图标源，子工程只通过 MSBuild `Link` 引用，不维护图标副本。
- 运行时帮助、Markdown 示例、内置备忘录、设计说明等业务文档按功能保留；仓库级入口文档使用根目录 `README.md` 和 `UpdateLog.md`。

## 简介

`CodeWF.EventBus` 是一个轻量的进程内事件总线库，适合在 WPF、WinForms、Avalonia UI、ASP.NET Core 和控制台程序中做模块解耦。

它支持两类典型场景：

1. `Command` 命令分发。
2. `Query<T>` 查询回传，方便实现简单 CQRS。

如果你熟悉 `MediatR`、`Prism.Events` 或 `MASA Framework` 的事件处理方式，可以把它理解成一个更轻量、对项目类型约束更少的选择。

设计说明可查看：

- [CodeWF.EventBus 设计文档](./docs/CodeWF.EventBus设计文档.md)

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
3. [Messenger   MvvmCross](https://www.mvvmcross.com/documentation/plugins/messenger?scroll=1000)
4. [MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)

## 一键打包

在仓库根目录运行 `pack.bat`，脚本会执行 `dotnet restore` 和 Release 构建，并把生成的 NuGet 包输出到 `Output\NuGet`。

## 第三方开源组件审计（2026-05-20）

检查方式：NuGet 元数据、恢复后的 `project.assets.json`、NuGet.org 与源码仓库信息。优先接受 MIT / Apache-2.0 / BSD；其它开源协议在源码与传递依赖均可追溯时单独标注通过。

整改：

- 包版本提升到 `3.4.5.5`，用于发布本次依赖升级与审计修正。
- `CodeWF.EventBus` 核心包已补充 `PackageLicenseExpression=MIT`，核心包无第三方运行时依赖。
- `CodeWF.DryIoc.EventBus` 将 `Prism.Core` 从 `9.0.537` 降到 MIT 的 `8.1.97`，避开 Prism 9 的 Community/Commercial License。
- `CodeWF.DryIoc.EventBus` 将 `DryIoc` 从 `6.0.0-preview-08` 改为稳定版 `5.4.3`。
- Avalonia AOT 示例升级到 Avalonia `12.0.3`、`ReactiveUI.Avalonia 12.0.1`、`Semi.Avalonia 12.0.1`、`CodeWF.LogViewer.Avalonia 12.0.3.1`。
- 移除 Debug-only 的 `Avalonia.Diagnostics` 引用；该包当前最新稳定版仍为 `11.3.16`，没有 Avalonia 12 对应稳定包，且示例代码未使用诊断 API。
- `Tmds.DBus.Protocol` 从 Avalonia 传递依赖 `0.92.0` pin 到 `0.93.0`。
- 测试依赖升级到 `Microsoft.NET.Test.Sdk 18.5.1`、`coverlet.collector 10.0.1`；`xunit.runner.visualstudio` 保持稳定 `3.1.5`，不使用 `4.0.0-pre.4` 预览版。

  包   使用范围   协议   源码/项目地址   结论  
  ---   ---   ---   ---   ---  
  `DryIoc`   DryIoc 扩展包   MIT   https://github.com/dadhi/DryIoc   通过  
  `Prism.Core` `8.1.97`   DryIoc/Prism 扩展包   MIT   https://github.com/PrismLibrary/Prism   通过，保留 8.x 开源线  
  `Swashbuckle.AspNetCore`   Web API 示例   MIT   https://github.com/domaindrivendev/Swashbuckle.AspNetCore   通过  
  `Avalonia` / `Avalonia.Desktop` / `Avalonia.Fonts.Inter` `12.0.3`   AOT 示例   MIT   https://github.com/AvaloniaUI/Avalonia   通过  
  `ReactiveUI.Avalonia` `12.0.1`   AOT 示例   MIT   https://github.com/reactiveui/reactiveui   通过，使用匹配 Avalonia 12 的包线  
  `Semi.Avalonia` `12.0.1`   AOT 示例   MIT   https://github.com/irihitech/Semi.Avalonia   通过，仅使用开源主体包  
  `CodeWF.Log.Core` / `CodeWF.LogViewer.Avalonia` `12.0.3.1`   AOT 示例日志   MIT   https://github.com/dotnet9/CodeWF.LogViewer   自研开源包  
  `CodeWF.Tools.Core` `1.3.13`   日志组件传递依赖 pin   MIT   https://github.com/dotnet9/CodeWF.Tools   自研开源包  
  `Tmds.DBus.Protocol` `0.93.0`   Avalonia Linux DBus 传递依赖   MIT   https://github.com/tmds/Tmds.DBus   通过，pin 到当前稳定版  
  `Microsoft.NET.Test.Sdk` `18.5.1`   测试   MIT   https://github.com/microsoft/vstest   通过  
  `coverlet.collector` `10.0.1`   测试覆盖率   MIT   https://github.com/coverlet-coverage/coverlet   通过  
  `xunit` / `xunit.runner.visualstudio`   测试   Apache-2.0   https://github.com/xunit/xunit   通过  

传递依赖检查结论：恢复后的有效依赖链未发现黑盒包、Prism 9 商业协议包、`AvaloniaUI.DiagnosticsSupport`、`Semi.Avalonia.*` 黑盒扩展或预发布 DryIoc 运行时依赖。`DryIoc 5.4.3` 构建时会在包内源码触发 `SYSLIB0051` 过时 API 警告，但不是许可证或已知漏洞告警。
## 包版本维护约定

XML 文件统一使用两个空格缩进。`Directory.Packages.props` 统一承载 NuGet 中央包管理开关和包版本变量，包括 `AvaloniaVersion` 等共享版本属性；`Directory.Build.props` 仅保留项目构建、编译选项和 NuGet 元数据。仓库如引用 `VC-LTL`、`YY-Thunks`，这两个兼容旧版操作系统的特殊包必须使用最新预览版。
