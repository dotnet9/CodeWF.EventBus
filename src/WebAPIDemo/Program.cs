using CodeWF.EventBus;
using Messages.Services;
using WebAPIDemo.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<ITimeService, TimeService>();

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

EventBusExtensions.UseEventBus((t) => app.Services.GetRequiredService(t), typeof(Program).Assembly);

app.Run();