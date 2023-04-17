using Discord.WebSocket;

namespace HTWDiscordBot.Services
{
    internal class SlashCommandService
    {
        private DiscordService discordService;
        private HTWService htwService;

        public SlashCommandService(DiscordService discordService, HTWService htwService)
        {
            this.discordService = discordService;
            this.htwService = htwService;
        }

        public Task InitializeAsync()
        {
            discordService.client.SlashCommandExecuted += SlashCommandHandler;
            return Task.CompletedTask;
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "start":
                    await HandleStartCommand(command);
                    break;
                case "stop":
                    await HandleStopCommand(command);
                    break;
            }
        }

        private async Task HandleStartCommand(SocketSlashCommand command)
        {
            await command.RespondAsync("Started");
            await htwService.SetShouldCheck(true);
        }

        private async Task HandleStopCommand(SocketSlashCommand command)
        {
            await command.RespondAsync("Stopped");
            await htwService.SetShouldCheck(false);
        }
    }
}
