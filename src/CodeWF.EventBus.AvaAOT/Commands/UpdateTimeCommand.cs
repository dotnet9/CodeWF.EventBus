namespace CodeWF.EventBus.AvaAOT.Commands;

public class UpdateTimeCommand(string time) : Command
{
    public string? Time { get; set; } = time;
}