# CodeWF.EventBus

## 1. 简介

EventBus(事件总线)，用于解耦模块之间的通讯。本库（[CodeWF.EventBus](https://www.nuget.org/packages?q=CodeWF.EventBus)）适用于进程内消息传递（无其他外部依赖），与大家普遍使用的[MediatR](https://github.com/jbogard/MediatR)部分类似，但[MediatR](https://github.com/jbogard/MediatR)库侧重于[ASP.NET Core](https://learn.microsoft.com/zh-cn/aspnet/core/?view=aspnetcore-9.0)设计使用，而本库也有点点优势：

1. 设计可在各种模板项目使用，如WPF、Winform、AvaloniaUI、ASP.NET Core等，主要参考了[Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)设计;
2. 参考[MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)增强消息处理能力：

```CSharp
internal class MessageHandler
{
    [EventHandler(Order = 3)]
    private void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
    {
        AddLog($"收到自动订阅消息3({message}”");
    }

    [EventHandler(Order = 1)]
    private void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"收到自动订阅消息({message}”");
    }

    [EventHandler(Order = 2)]
    private void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
    {
        AddLog($"收到自动订阅消息2({message}”");
    }
}
```

## 2. 怎么使用事件总线？

首先定义消息类，消息需要继承自`CodeWF.EventBus.Message`：

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
        return $"创建产品消息-》产品名称：{Name}";
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
        return $"删除产品消息-》产品Id：{Id}";
    }
}
```

定义好消息，这里我们有两种方式使用事件总线，非IOC和IOC方式：

1. 非IOC方式：需要安装`CodeWF.EventBus`包，适用于未使用IOC的模板程序，比如WPF、Winform、AvaloniaUI、控制台程序，当然ASP.NET Core也能用。
2. IOC方式：需要安装`CodeWF.AspNetCore.EventBus`包，适合于在ASP.NET Core程序中使用。

### 2.1. 非IOC方式使用

适合于未使用IOC方式使用事件总线，比如在WPF、Winform、AvaloniaUI、控制台等程序中直接使用事件帮助类的静态实例，下面是使用步骤。

创建项目（不限于项目类型，比如控制台程序），通过NuGet引入`CodeWF.EventBus`包：

```shell
Install-Package CodeWF.EventBus -Version 1.0.1
```

创建消息处理程序，这里参考了[Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)设计，可订阅消息、取消订阅消息、发布消息，适合于手工指定处理方法：


```CSharp
internal class MessageHandler
{
    internal void ManuSubscribe()
    {
        Messenger.Default.Subscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
        Messenger.Default.Subscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
    }

    internal void ManuUnsubscribe()
    {
        Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
        Messenger.Default.Unsubscribe<DeleteProductMessage>(this, ReceiveManuDeleteProductMessage);
    }

    internal void Publish()
    {
        Messenger.Default.Publish(this, new CreateProductMessage(this, $"{DateTime.Now:HHmmss}号产品"));
        Messenger.Default.Publish(this, new DeleteProductMessage(this, $"{DateTime.Now:HHmmss}号"));
    }

    void ReceiveManuCreateProductMessage(CreateProductMessage message)
    {
        AddLog($"收到手动注册的{message}");
    }

    void ReceiveManuDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"收到手动注册的{message}");
    }

    private void AddLog(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
    }
}
```

最后是消息使用：

```CSharp
using ConsoleDemo.EventBus;

var handler = new MessageHandler();

Console.WriteLine("1、未注册时发布消息：");
handler.Publish();
Console.WriteLine();

Console.WriteLine("2、手动注册后发布消息：");
handler.ManuSubscribe();
handler.Publish();

Console.WriteLine("3、取消手动注册后发布消息：");
handler.ManuUnsubscribe();
handler.Publish();

Console.ReadKey();
```

如果消息较多，也可使用自动注册消息处理方法，我们修改处理程序：

```CSharp
internal class MessageHandler
{
    internal void AutoSubscribe()
    {
        Messenger.Default.Subscribe(this);
    }

    internal void AutoUnsubscribe()
    {
        Messenger.Default.Unsubscribe(this);
    }

    internal void Publish()
    {
        Messenger.Default.Publish(this, new CreateProductMessage(this, $"{DateTime.Now:HHmmss}号产品"));
        Messenger.Default.Publish(this, new DeleteProductMessage(this, $"{DateTime.Now:HHmmss}号"));
    }

    [EventHandler(Order = 3)]
    private void ReceiveAutoCreateProductMessage3(CreateProductMessage message)
    {
        AddLog($"收到自动订阅消息3({message}”");
    }

    [EventHandler(Order = 1)]
    private void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"收到自动订阅消息({message}”");
    }

    [EventHandler(Order = 2)]
    private void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
    {
        AddLog($"收到自动订阅消息2({message}”");
    }

    private void AddLog(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
    }
}
```

`[EventHandler(Order = 0)]` 定义消息的执行顺序。每个消息都可以匹配多个处理程序。一个类中可以有多个消息处理方法，可以订阅同一个消息，也可以订阅不同的消息。

支持消息处理程序的注销：

1. 注销指定处理程序：`Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage)`
2. 注销指定类的所有处理程序：`Messenger.Default.Unsubscribe(this)`

消息使用：

```CSharp
using ConsoleDemo.EventBus;

var handler = new MessageHandler();

Console.WriteLine("1、未注册时发布消息：");
handler.Publish();
Console.WriteLine();

Console.WriteLine("2、自动注册后发布消息：");
handler.AutoSubscribe();
handler.Publish();

Console.WriteLine("3、取消自动注册后发布消息：");
handler.AutoUnsubscribe();
handler.Publish();

Console.ReadKey();
```

### 2.2. IOC方式使用

适合于在ASP.NET Core程序中使用，下面是使用步骤。

创建项目（ASP.NET Core模块项目，比如Web API、MVC、Razor Page、Blazor Server等），通过NuGet引入`CodeWF.AspNetCore.EventBus`:

```shell
Install-Package CodeWF.AspNetCore.EventBus -Version 1.0.1
```

创建消息处理程序，处理类中可以正常使用构造函数注入IOC服务：

```CSharp
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
        AddLog($"收到消息3({message}”");
    }

    [EventHandler(Order = 1)]
    public void ReceiveAutoDeleteProductMessage(DeleteProductMessage message)
    {
        AddLog($"收到消息({message}”");
    }

    [EventHandler(Order = 2)]
    public void ReceiveAutoCreateProductMessage2(CreateProductMessage message)
    {
        AddLog($"收到消息2({message}”");
    }

    private void AddLog(string message)
    {
        Console.WriteLine($"{timeService.GetTime()}: {message}\r\n");
    }
}
public interface ITimeService
{
    string GetTime();
}

public class TimeService : ITimeService
{
    public string GetTime()
    {
        return DateTime.Now.ToString("HH:mm:ss fff");
    }
}
```

在Program中注册事件总线：

```CSharp
using CodeWF.AspNetCore.EventBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 用于测试事件处理类正常使用IOC功能
builder.Services.AddSingleton<ITimeService, TimeService>();

// 1、注册事件总线，将标注`EventHandler`特性方法的类采用单例方式注入IOC容器
builder.Services.AddEventBus(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// 2、将上面已经注入IOC容器的类取出、关联处理方法到事件总线管理
app.UseEventBus(typeof(Program).Assembly);

app.Run();
```

在控制器或其他服务可以发布消息，上面的处理程序会接收处理：

```CSharp
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

## 3. 总结

CodeWF.EventBus，一款灵活的事件总线库，实现模块间解耦通信。支持多种.NET项目类型，如WPF、WinForms、ASP.NET Core等。采用简洁设计，轻松实现事件的发布与订阅。通过有序的消息处理，确保事件得到妥善处理。简化您的代码，提升系统可维护性。立即体验CodeWF.EventBus，让事件处理更加高效！

仓库地址是https://github.com/dotnet9/CodeWF.EventBus，开发过程中参考不少开源项目，他们是：

1. [Prism.Events](https://github.com/PrismLibrary/Prism/tree/master/src/Prism.Events)
2. [MediatR](https://github.com/jbogard/MediatR)
2. [MASA Framework](https://docs.masastack.com/framework/tutorial/mf-part-3#section-69828ff0)