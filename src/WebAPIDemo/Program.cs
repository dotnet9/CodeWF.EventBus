using CodeWF.AspNetCore.EventBus;
using CommandAndQueryModel.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductService, ProductService>();

// 注册事件总线，并自动扫描当前调用程序集中的实例处理器。
builder.Services.AddEventBus();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// 在应用启动阶段把处理器真正接入到事件总线。
app.UseEventBus();

app.Run();
