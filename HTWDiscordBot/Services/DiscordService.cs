using Discord;
using Discord.WebSocket;

namespace HTWDiscordBot.Services
{
    public class DiscordService
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

        //Wird ausgeführt, wenn der Bot bereit ist
        private async Task Client_ReadyAsync()
        {
            await client.SetGameAsync("Hack The Web", type: ActivityType.Playing);
        }

        //Konfiguriert die DiscordSocketConfig
        public static DiscordSocketConfig CreateDiscordSockteConfig()
        {
            DiscordSocketConfig discordSocketConfig = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences,
                LogGatewayIntentWarnings = false,
                DefaultRatelimitCallback = info => RatelimitCallback(info)
            };

            return discordSocketConfig;
        }

        private static async Task RatelimitCallback(IRateLimitInfo info)
        {
            if (info.Remaining < 1)
                Console.WriteLine($"[RateLimit/{LogSeverity.Warning}] Global: {info.IsGlobal}, Limit: {info.Limit}, Remaining: {info.Remaining}, RetryAfter: {info.RetryAfter}, ResetsAfter: {info.ResetAfter?.TotalSeconds}, Lag: {info.Lag?.TotalMilliseconds}");
        }
    }
}
