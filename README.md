# CodeWF.EventBus

EventBus，事件总线，进程内事件订阅与发布，可在各种模板项目使用：WPF、Winform、AvaloniaUI、ASP.NET Core

1. 安装

```shell
Install-Package CodeWF.EventBus -Version 1.0.0
```

2. 定义消息

```CSharp
public class SayHelloMessage : CodeWF.EventBus.Message
{
    public string Word { get; }

    public SayHelloMessage(object sender, string word) : base(sender)
    {
        Word = word;
    }
}
```

3. 注册特定消息处理方法

```CSharp
public class EventHandle
{
    public EventHandle()
    {
        Messenger.Default.Subscribe<SayHelloMessage>(this, ReceiveManuMessage);
    }

    private void ReceiveManuMessage(SayHelloMessage message)
    {
        AddLog($"收到手工订阅消息“{message.Word}”");
    }
}
```

4. 自动注册消息处理方法

```CSharp
public class EventHandle
{
    public EventHandle()
    {
        Messenger.Default.Unsubscribe(this);
    }

[EventHandler(Order = 2)]
private void ReceiveAutoMessage1(SayHelloMessage message)
{
    AddLog($"收到自动订阅消息({nameof(ReceiveAutoMessage1)})“{message.Word}”");
}

[EventHandler(Order = 1)]
private void ReceiveAutoMessage2(SayHelloMessage message)
{
    AddLog($"收到自动订阅消息({nameof(ReceiveAutoMessage2)})“{message.Word}”");
}

[EventHandler(Order = 3)]
private void ReceiveAutoMessage3(SayHelloMessage message)
{
    AddLog($"收到自动订阅消息({nameof(ReceiveAutoMessage3)})“{message.Word}”");
}
}
```
