# CodeWF.EventBus

## 1. 简介

EventBus(事件总线)，用于解耦模块之间的通讯。本库（[CodeWF.EventBus](https://www.nuget.org/packages?q=CodeWF.EventBus)）适用于进程内消息传递（无其他外部依赖），与大家普遍使用的[MediatR](https://github.com/jbogard/MediatR)通知功能类似，但[MediatR](https://github.com/jbogard/MediatR)库侧重于[ASP.NET Core](https://learn.microsoft.com/zh-cn/aspnet/core/?view=aspnetcore-9.0)设计使用，本库优势：

1. 设计可在各种模板项目使用，如WPF、Winform、Avalonia UI、ASP.NET Core等。
2. 支持使用了IOC容器的项目，当然也支持未使用任何IOC容器的模板项目。
3. 参考[MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)增强消息处理能力：

```CSharp
[Event]
public class MessageHandler
{
    private readonly ITimeService timeService;

    public MessageHandler(ITimeService timeService)
    {
        this.timeService = timeService;
    }

    [EventHandler(Order = 3)]
    public void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
    {
        AddLog($"MessageHandler Received message 3 \"{message}\"");
    }

    [EventHandler(Order = 1)]
    public void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"MessageHandler Received message \"{message}\"");
    }

    [EventHandler(Order = 2)]
    public void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
    {
        AddLog($"MessageHandler Received message 2 \"{message}\"");
    }

    private void AddLog(string message)
    {
        Console.WriteLine($"{timeService.GetTime()}: {message}\r\n");
    }
}
```

## 2. 怎么使用事件总线？

首先请搜索NuGet包`CodeWF.EventBus`并安装，下面细说使用方法。

### 2.1. 添加事件总线

#### 2.1.1. 使用了IOC

如果是ASP.NET Core程序，比如MVC、Razor Pages、Blazor Server等，，在`Program`中添加如下代码：

```csharp
// ....

// 1、注册事件总线，将标注`EventHandler`特性方法的类采用单例方式注入IOC容器
EventBusExtensions.AddEventBus(
    (t1, t2) => builder.Services.AddSingleton(t1, t2),
    t => builder.Services.AddSingleton(t),
    typeof(Program).Assembly);

var app = builder.Build();

// ...

// 2、将上面已经注入IOC容器的类取出、关联处理方法到事件总线管理
EventBusExtensions.UseEventBus((t) => app.Services.GetRequiredService(t), typeof(Program).Assembly);

// ...
```

- `AddEventBus`方法会扫描传入的程序集列表，将标注`Event`特性的类下又标注`EventHandler`特性方法的类采用单例方式注入IOC容器。
- `UseEventBus`方法会将上一步注入的类通过IOC单例获取到实例，将实例的消息处理方法注册到消息管理队列中去，待收到消息发布时，会从消息管理队列中查找消息处理方法并调用，达到消息通知的功能。

如果使用的其他IOC，比如WPF中使用了Prism框架，写法如下：

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

根据IOC注册单例、获取服务的API不同，做相应修改即可。

#### 2.1.2. 未使用IOC

未使用IOC容器，比如默认的WPF、Winform、AvaloniaUI、控制台程序不包含IOC容器，不做事件服务注册操作，功能使用上和使用IOC只差自动订阅功能，其他功能一样。

### 2.2. 定义消息（事件类型）

首先定义消息类，即需要发布或订阅的事件类型，消息需要继承自`CodeWF.EventBus.Message`：

```CSharp
public class CreateProductMessage : CodeWF.EventBus.Message
{
    public string Name { get; }

    public CreateProductMessage(object sender, string name) : base(sender)
    {
        Name = name;
    }

    public override string ToString()
    {
        return $"Create product message ->Product name:{Name}";
    }
}

public class DeleteProductMessage : CodeWF.EventBus.Message
{
    public string Id { get; }

    public DeleteProductMessage(object sender, string id) : base(sender)
    {
        Id = id;
    }

    public override string ToString()
    {
        return $"Delete product message ->Product ID：{Id}";
    }
}
```

### 2.3. 订阅消息（事件）

#### 2.3.1. 自动订阅

在`B/S`程序中，一般将事件处理程序单独封装到一个类中，如下代码：

```csharp
[Event]
public class MessageHandler
{
    private readonly ITimeService timeService;

    public MessageHandler(ITimeService timeService)
    {
        this.timeService = timeService;
    }

    [EventHandler(Order = 3)]
    public void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
    {
        AddLog($"MessageHandler Received message 3 \"{message}\"");
    }

    [EventHandler(Order = 1)]
    public void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"MessageHandler Received message \"{message}\"");
    }

    [EventHandler(Order = 2)]
    public void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
    {
        AddLog($"MessageHandler Received message 2 \"{message}\"");
    }
}
```

- 类`MessageHandler`添加了`Event`特性，在IOC注入时标识为可以做为单例注入。
- 标注了`EventHandler`特性的方法拥有处理消息的能力，该方法只能有一个事件类型参数。

使用IOC容器的程序会自动将标注`Event`特性的类做为单例注入容器，事件总线收到消息通知时自动查找标注`EventHandle`特性的方法进行调用，达到消息通知的功能。

#### 2.3.2. 手动订阅

对于未标注`Event`特性的类，可手动注册消息处理程序，如下图是未使用IOC，手动注册示例：

```csharp
internal class MessageHandler
{
    internal void ManuSubscribe()
    {
        Messenger.Default.Subscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
        Messenger.Default.Subscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
    }

    void ReceiveManuCreateProductMessage(CreateProductMessage message)
    {
        AddLog($"Received manually registered \"{message}\"");
    }

    void ReceiveManuDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"Received manually registered \"{message}\"");
    }
}
```

上面挨个注册处理方法有时会过于啰嗦，可以简化：

```csharp
internal class MessageHandler
{
    internal void AutoSubscribe()
    {
        Messenger.Default.Subscribe(this);
    }

    [EventHandler(Order = 3)]
    private void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
    {
        AddLog($"Received automatic subscription message 3 \"({message}\"");
    }

    [EventHandler(Order = 1)]
    private void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"Received automatic subscription message \"{message}\"");
    }

    [EventHandler(Order = 2)]
    private void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
    {
        AddLog($"Received automatic subscription message 2 \"{message}\"");
    }
}
```

使用了IOC，可以注入`IMessenger`服务替换`Messenger.Default`使用，`Messenger`是`IMessenger`接口的默认实现，`Messenger.Default`是单例引用。

```csharp
public class MessageTestViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;

    public MessageTestViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        _messenger.Subscribe(this);
    }

    [EventHandler]
    public void ReceiveEventBusMessage(TestMessage message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"模块【Test】收到{nameof(TestMessage)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }
}
```

手动订阅可以在WPF的ViewModel中使用（代码如上），也可以在IOC其他生命周期的服务中使用：

```csharp
public class TimeService : ITimeService
{
    private readonly IMessenger _messenger;

    public TimeService(IMessenger messenger)
    {
        _messenger = messenger;

        _messenger.Subscribe(this);
    }


    [EventHandler]
    public void ReceiveEventBusMessage(TestMessage message)
    {
    }
}
```

手动注册可运用在无法或不需要单例注入的情况使用。

### 2.4. 发布消息

发布就比较简单：

```csharp
_messenger.Publish(this, new TestMessage(this, nameof(MessageTestViewModel), TestClass.CurrentTime()));
```

比如在`B/S`控制器的`Action`使用：

```csharp
[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;
    private readonly IMessenger _messenger;

    public EventController(ILogger<EventController> logger, IMessenger messenger)
    {
        _logger = logger;
        _messenger = messenger;
    }

    [HttpPost]
    public void Add()
    {
        _messenger.Publish(this, new CreateProductMessage(this, $"{DateTime.Now:HHmmss}号产品"));
    }

    [HttpDelete]
    public void Delete()
    {
        _messenger.Publish(this, new DeleteProductMessage(this, $"{DateTime.Now:HHmmss}号"));
    }
}
```

在`WPF/Avalonia UI`的`ViewModel`中使用:

```csharp
public class MessageTestViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;

    public MessageTestViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public Task ExecuteEventBusAsync()
    {
        _messenger.Publish(this, new TestMessage(this, nameof(MessageTestViewModel), TestClass.CurrentTime()));
        return Task.CompletedTask;
    }
}
```

### 2.5. 取消订阅消息（事件）

支持消息处理程序的注销：

1. 注销指定处理程序：`Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage)`
2. 注销指定类的所有处理程序：`Messenger.Default.Unsubscribe(this)`

## 3. 总结

CodeWF.EventBus，一款灵活的事件总线库，实现模块间解耦通信。支持多种.NET项目类型，如Avalonia UI、WPF、WinForms、ASP.NET Core等。采用简洁设计，轻松实现事件的发布与订阅。通过有序的消息处理，确保事件得到妥善处理。

简化您的代码，提升系统可维护性。

立即体验CodeWF.EventBus，让事件处理更加高效！

仓库地址是https://github.com/dotnet9/CodeWF.EventBus，开发过程中参考不少开源项目，他们是：

1. [Messenger | MvvmCross](https://www.mvvmcross.com/documentation/plugins/messenger?scroll=1000)
2. [Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)
3. [MediatR](https://github.com/jbogard/MediatR)
4. [MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)