# CodeWF.EventBus

EventBus，事件总线，进程内事件订阅与发布，可在各种模板项目使用：WPF、Winform、AvaloniaUI、ASP.NET Core。

消息需要继承自`CodeWF.EventBus.Message`，比如：

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
```

## 1. 非IOC方式使用

适合于未使用IOC方式使用事件总线，比如在WPF、Winform、AvaloniaUI、控制台等程序中直接使用事件帮助类的静态实例，下面是使用步骤。

### 1.1. 安装包

```shell
Install-Package CodeWF.EventBus -Version 1.0.1
```

### 1.2. 注册特定消息处理方法

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

```CSharp
internal class MessageHandler
{
    internal void ManuSubscribe()
    {
        Messenger.Default.Subscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
    }

    internal void ManuUnsubscribe()
    {
        Messenger.Default.Unsubscribe<CreateProductMessage>(this, ReceiveManuCreateProductMessage);
    }

    internal void Publish()
    {
        Messenger.Default.Publish(this, new CreateProductMessage(this, $"{DateTime.Now:HHmmss}号产品"));
    }

    void ReceiveManuCreateProductMessage(CreateProductMessage message)
    {
        AddLog($"收到手动注册的{message}");
    }

    private void AddLog(string message)
    {
        Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
    }
}
```

### 1.3. 自动注册消息处理方法

```CSharp
using ConsoleDemo.EventBus;

var handler = new MessageHandler();

Console.WriteLine("1、未注册时发布消息：");
handler.Publish();
Console.WriteLine();

Console.WriteLine("4、自动注册后发布消息：");
handler.AutoSubscribe();
handler.Publish();

Console.WriteLine("5、取消自动注册后发布消息：");
handler.AutoUnsubscribe();
handler.Publish();

Console.ReadKey();
```

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

## 1. IOC方式使用

适合于在ASP.NET Core程序中使用，下面是使用步骤。

### 1.1. 安装包

```shell
Install-Package CodeWF.AspNetCore.EventBus -Version 1.0.1
```

### 1.2. 注册事件总线

```CSharp
using CodeWF.AspNetCore.EventBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1、注册事件总线，将标注`EventHandler`特性方法的类采用单例方式注入IOC容器
builder.Services.AddEventBus();

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
app.UseEventBus();

app.Run();
```

### 1.3. 定义事件处理类

处理类中可以正常使用构造函数注入IOC服务：

```CSharp
public class MessageHandler
{
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
        Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} {message}\r\n");
    }
}
```

### 1.4. 控制器中发送消息

控制器或其他服务可以发布消息，上面的处理程序会接收处理：

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