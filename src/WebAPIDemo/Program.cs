using CodeWF.AspNetCore.EventBus;
using CommandAndQueryModel.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductService, ProductService>();

// 显式指定处理器程序集，兼容 NativeAOT 和裁剪发布。
var handlerAssembly = typeof(Program).Assembly;
builder.Services.AddEventBus(handlerAssembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// 在应用启动阶段把处理器真正接入到事件总线。
app.UseEventBus(handlerAssembly);

app.Run();
