using Discord.WebSocket;

namespace HTWDiscordBot.Services
{
    //Regestriert Slash Commands
    internal class SlashCommandService
    {
        private readonly DiscordService discordService;
        private readonly HTWService htwService;

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

        //Wird ausgeführt, wenn ein Slash Command ausgeführt wird
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

        //Startet die Überprüfung auf neue Aufgaben
        private async Task HandleStartCommand(SocketSlashCommand command)
        {
            await command.RespondAsync("Started");
            await htwService.SetShouldCheck(true);
        }

        //Stoppt die Überprüfung auf neue Aufgaben
        private async Task HandleStopCommand(SocketSlashCommand command)
        {
            await command.RespondAsync("Stopped");
            await htwService.SetShouldCheck(false);
        }
    }
}
