using CodeWF.EventBus;
using WebAPIDemo.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 用于测试事件处理类正常使用IOC功能
builder.Services.AddSingleton<ITimeService, TimeService>();

// 1、注册事件总线，将标注`EventHandler`特性方法的类采用单例方式注入IOC容器
EventBusExtensions.AddEventBus(
    (t1, t2) => builder.Services.AddSingleton(t1, t2),
    t => builder.Services.AddSingleton(t),
    typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// 2、将上面已经注入IOC容器的类取出、关联处理方法到事件总线管理
EventBusExtensions.UseEventBus((t) => app.Services.GetRequiredService(t), typeof(Program).Assembly);

app.Run();