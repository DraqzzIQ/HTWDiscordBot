using Discord;

namespace HTWDiscordBot.Services
{
    //Stellt eine Log-Methode bereit
    internal class LoggingService
    {
        private readonly DiscordService discordService;

        public LoggingService(DiscordService discordService)
        {
            this.discordService = discordService;
        }

        public Task InitializeAsync()
        {
            discordService.client.Log += LogAsync;
            return Task.CompletedTask;
        }

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}
