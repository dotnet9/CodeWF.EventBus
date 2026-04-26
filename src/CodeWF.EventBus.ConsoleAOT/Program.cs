using CodeWF.EventBus;

var eventBus = new EventBus();

eventBus.Subscribe<PingCommand>(OnPing);
eventBus.Subscribe<PingQuery>(query => query.Result = "pong");

await eventBus.PublishAsync(new PingCommand("hello from AOT"));
var pong = await eventBus.QueryAsync(new PingQuery());

Console.WriteLine($"Query result: {pong}");

static void OnPing(PingCommand command)
{
    Console.WriteLine($"Received command: {command.Message}");
}

sealed class PingCommand : Command
{
    public PingCommand(string message)
    {
        Message = message;
    }

    public string Message { get; }
}

sealed class PingQuery : Query<string?>
{
    public override string? Result { get; set; }
}
