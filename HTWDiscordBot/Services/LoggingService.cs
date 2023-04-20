using Discord;

namespace HTWDiscordBot.Services
{
    //Stellt eine Log-Methode bereit
    public class LoggingService
    {
        public Task LogAsync(LogMessage message)
        {
            Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}