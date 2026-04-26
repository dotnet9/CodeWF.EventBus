using CodeWF.EventBus;

var eventBus = new EventBus();

// 直接订阅委托是最简单的无 IOC 使用方式。
eventBus.Subscribe<PingCommand>(OnPing);
eventBus.Subscribe<PingQuery>(query => query.Result = "pong");

await eventBus.PublishAsync(new PingCommand("hello from AOT"));
var pong = await eventBus.QueryAsync(new PingQuery());

Console.WriteLine($"Query result: {pong}");

static void OnPing(PingCommand command)
{
    Console.WriteLine($"Received command: {command.Message}");
}

/// <summary>
/// AOT 示例中的命令消息。
/// </summary>
sealed class PingCommand : Command
{
    public PingCommand(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

/// <summary>
/// AOT 示例中的查询消息。
/// </summary>
sealed class PingQuery : Query<string?>
{
    public override string? Result { get; set; }
}
