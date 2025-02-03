using Discord;
using Discord.WebSocket;

namespace HtwDiscordBot.Services;

public class DiscordService(ConfigService configService, DiscordSocketClient client)
{
    public async Task InitializeAsync()
    {
        client.Ready += Client_ReadyAsync;
        await client.LoginAsync(TokenType.Bot, configService.Config.Token);
        await client.StartAsync();
    }

    private async Task Client_ReadyAsync()
    {
        await client.SetGameAsync("Hack The Web", type: ActivityType.Playing);
    }

    public static DiscordSocketConfig CreateDiscordSocketConfig()
    {
        DiscordSocketConfig discordSocketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildPresences |
                             GatewayIntents.MessageContent,
            LogGatewayIntentWarnings = false,
            DefaultRatelimitCallback = RatelimitCallback
        };

        return discordSocketConfig;
    }

    private static async Task RatelimitCallback(IRateLimitInfo info)
    {
        if (info.Remaining < 1)
            Console.WriteLine(
                $"[RateLimit/{LogSeverity.Warning}] Global: {info.IsGlobal}, Limit: {info.Limit}, Remaining: {info.Remaining}, RetryAfter: {info.RetryAfter}, ResetsAfter: {info.ResetAfter?.TotalSeconds}, Lag: {info.Lag?.TotalMilliseconds}");
    }
}