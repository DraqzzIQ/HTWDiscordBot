using Discord;
using Discord.WebSocket;

namespace HTWDiscordBot.Services
{
    internal class DiscordService
    {
        private readonly DiscordSocketClient client;
        private readonly ConfigService configService;

        public DiscordService(ConfigService configService, DiscordSocketClient client)
        {
            this.configService = configService;
            this.client = client;
        }

        public async Task InitializeAsync()
        {
            client.Ready += Client_ReadyAsync;
            await client.LoginAsync(TokenType.Bot, configService.Config.Token);
            await client.StartAsync();
        }

        //Konfiguriert die DiscordSocketConfig
        public static DiscordSocketConfig CreateDiscordSockteConfig()
        {
            DiscordSocketConfig discordSocketConfig = new();
            discordSocketConfig.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers;
            discordSocketConfig.LogGatewayIntentWarnings = false;
            return discordSocketConfig;
        }

        //Wird ausgeführt, wenn der Bot bereit ist
        private async Task Client_ReadyAsync()
        {
            await client.SetGameAsync("Hack The Web", type: ActivityType.Playing);
        }
    }
}
