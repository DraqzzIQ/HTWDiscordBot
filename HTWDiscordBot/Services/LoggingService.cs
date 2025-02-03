using Discord;

namespace HtwDiscordBot.Services;

public class LoggingService
{
    public Task LogAsync(LogMessage message)
    {
        Console.WriteLine($"[General/{message.Severity}] {message}");

        return Task.CompletedTask;
    }

    public void Log(LogMessage message)
    {
        Console.WriteLine($"[General/{message.Severity}] {message}");
    }
}