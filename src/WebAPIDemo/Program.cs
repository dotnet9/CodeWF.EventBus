using CodeWF.EventBus;
using WebAPIDemo.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ���ڲ����¼�����������ʹ��IOC����
builder.Services.AddSingleton<ITimeService, TimeService>();

// 1��ע���¼����ߣ�����ע`EventHandler`���Է���������õ�����ʽע��IOC����
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

// 2���������Ѿ�ע��IOC��������ȡ�����������������¼����߹���
EventBusExtensions.UseEventBus((t) => app.Services.GetRequiredService(t), typeof(Program).Assembly);

app.Run();