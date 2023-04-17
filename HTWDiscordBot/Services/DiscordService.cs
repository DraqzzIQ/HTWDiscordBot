using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace HTWDiscordBot.Services
{
    internal class DiscordService
    {
        public DiscordSocketClient client { get; private set; }
        private ConfigService configService;
        private DiscordSocketConfig discordSocketConfig;

        public DiscordService(DiscordSocketConfig discordSocketConfig, ConfigService configService)
        {
            this.discordSocketConfig = discordSocketConfig;
            this.configService = configService;
        }

        public async Task InitializeAsync()
        {
            await ConfigureAsync();
            client = new DiscordSocketClient(discordSocketConfig);

            client.Ready += Client_Ready;
            await client.LoginAsync(TokenType.Bot, configService.Config.Token);
            await client.StartAsync();
        }

        private Task ConfigureAsync()
        {
            discordSocketConfig.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds;
            discordSocketConfig.LogGatewayIntentWarnings = false;
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            await ConfigureGlobalCommands();
            await client.SetGameAsync("Hack The Web", type: ActivityType.Playing);
        }
        private async Task ConfigureGlobalCommands()
        {
            var globalCommands = await client.GetGlobalApplicationCommandsAsync();

            var startCommand = new SlashCommandBuilder();
            startCommand.WithName("start");
            startCommand.WithDescription("Startet das Tracking der Aufgaben");

            var stopCommand = new SlashCommandBuilder();
            stopCommand.WithName("stop");
            stopCommand.WithDescription("Stoppt das Tracking der Aufgaben");

            try
            {
                if (!globalCommands.Any(x => x.Name == "start"))
                    await client.CreateGlobalApplicationCommandAsync(startCommand.Build());

                if (!globalCommands.Any(x => x.Name == "stop"))
                    await client.CreateGlobalApplicationCommandAsync(stopCommand.Build());
            }
            catch (HttpException exception)
            {
                // Wenn Command ungültig
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
