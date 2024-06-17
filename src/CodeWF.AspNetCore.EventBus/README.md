简化Web API事件总线引入，只需要引入一个包：

```shell
NuGet\Install-Package CodeWF.AspNetCore.EventBus
```

使用：

```csharp
using CodeWF.EventBus;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

Register EventBus
builder.Services.AddEventBus(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// Use EventBus
app.UseEventBus(Assembly.GetExecutingAssembly());

app.Run();
```