using CodeWF.AspNetCore.EventBus;
using WebAPIDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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