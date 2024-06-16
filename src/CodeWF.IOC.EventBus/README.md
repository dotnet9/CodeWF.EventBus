`CodeWF.EventBus`支持任意IOC容器的项目，请搜索 NuGet 包`CodeWF.IOC.EventBus`并安装最新版，安装完成后，根据 IOC 容器注册单例、获取服务的 API 不同，做相应修改即可。下面是`AddEventBus`和`UseEventBus`扩展方法逻辑：

```csharp
public static class EventBusExtensions
{
    public static void AddEventBus(Action<Type, Type> addSingleton1,
        Action<Type> addSingleton2, params Assembly[] assemblies)
    {
        addSingleton1(typeof(IEventBus), typeof(CodeWF.EventBus.EventBus));
        HandleCommandObject(addSingleton2, assemblies);
    }

    public static void UseEventBus(Func<Type, object> resolveAction, params Assembly[] assemblies)
    {
        if (!(resolveAction(typeof(IEventBus)) is IEventBus messenger))
        {
            throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
        }

        HandleCommandObject(type => messenger.Subscribe(resolveAction(type)),
            assemblies);
    }

    private static void HandleCommandObject(Action<Type> handleRecipient, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies.Concat(new []{Assembly.GetCallingAssembly()}))
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && t.GetCustomAttributes<EventAttribute>().Any()
                            && t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                            BindingFlags.NonPublic).Any(m =>
                                m.GetCustomAttributes<EventHandlerAttribute>().Any()));

            foreach (var type in types)
            {
                handleRecipient(type);
            }
        }
    }
}
```

比如上面的ASP.NET Core程序在安装`CodeWF.IOC.EventBus`包后，注册也可以这样写：

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