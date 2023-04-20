using Discord;

namespace HTWDiscordBot.Services
{
    //Stellt Logging Methoden bereit
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
}