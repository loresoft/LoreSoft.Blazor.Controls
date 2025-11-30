namespace Sample.Core.Models;

public class LogEvent
{
    public int Id { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public string Level { get; set; } = null!;

    public string Message { get; set; } = null!;
}
