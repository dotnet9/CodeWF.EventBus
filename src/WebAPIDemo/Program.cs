using CodeWF.AspNetCore.EventBus;
using WebAPIDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ���ڲ����¼�����������ʹ��IOC����
builder.Services.AddSingleton<ITimeService, TimeService>();

// 1��ע���¼����ߣ�����ע`EventHandler`���Է���������õ�����ʽע��IOC����
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

// 2���������Ѿ�ע��IOC��������ȡ�����������������¼����߹���
app.UseEventBus(typeof(Program).Assembly);

app.Run();