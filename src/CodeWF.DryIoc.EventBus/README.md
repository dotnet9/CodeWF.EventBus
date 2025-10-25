[![NuGet](https://img.shields.io/nuget/v/CodeWF.DryIoc.EventBus.svg)](https://www.nuget.org/packages/CodeWF.DryIoc.EventBus/)
[![NuGet](https://img.shields.io/nuget/dt/CodeWF.DryIoc.EventBus.svg)](https://www.nuget.org/packages/CodeWF.DryIoc.EventBus/)
[![License](https://img.shields.io/github/license/dotnet9/CodeWF.EventBus)](LICENSE)

简化`Prism.Container.DryIoc`事件总线引入，只需要引入一个包：

```shell
NuGet\Install-Package CodeWF.DryIoc.EventBus
```

使用：

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    IContainer? container = containerRegistry.GetContainer();

    // Register EventBus
    containerRegistry.AddEventBus(Assembly.GetExecutingAssembly());

    // ....

    // Use EventBus
    container.UseEventBus(Assembly.GetExecutingAssembly());
}
```