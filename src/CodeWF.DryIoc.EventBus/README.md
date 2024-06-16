简化`Prism.Container.DryIoc`事件总线引入，只需要引入一个包：

```shell
NuGet\Install-Package CodeWF.DryIoc.EventBus -Version 2.1.4
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