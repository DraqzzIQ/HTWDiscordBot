using Discord.Interactions;
using Discord.WebSocket;
using HTWDiscordBot.Handlers;
using HTWDiscordBot.Services;
using HTWDiscordBot.Services.HTW;
using Microsoft.Extensions.DependencyInjection;

namespace HTWDiscordBot
{
    public class Program
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient client;
        private readonly DiscordService discordService;
        private readonly LoggingService loggingService;
        private readonly HTWService htwService;
        private readonly InteractionHandler interactionHandler;

        public Program()
        {
            services = CreateProvider();
            client = services.GetRequiredService<DiscordSocketClient>();
            discordService = services.GetRequiredService<DiscordService>();
            loggingService = services.GetRequiredService<LoggingService>();
            htwService = services.GetRequiredService<HTWService>();
            interactionHandler = services.GetRequiredService<InteractionHandler>();
        }

        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            client.Ready += Client_ReadyAsync;
            client.Log += loggingService.LogAsync;
            await discordService.InitializeAsync();
            await interactionHandler.InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        //Wird ausgeführt, wenn der Bot bereit ist
        private async Task Client_ReadyAsync()
        {
            await htwService.InitializeAsync();
        }

        //ServiceProvider für Dependency Injection
        private static IServiceProvider CreateProvider()
        {
            IServiceCollection collection = new ServiceCollection()
                .AddSingleton<ConfigService>()
                .AddSingleton(DiscordService.CreateDiscordSockteConfig())
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<DiscordService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<LoggingService>()
                .AddSingleton<HTWService>()
                .AddSingleton<AuthentificationService>()
                .AddSingleton<ScoreboardService>()
                .AddSingleton<ChallengeService>()
                .AddSingleton<HttpClientService>()
                .AddSingleton<HtmlParserService>()
                .AddSingleton<InteractionHandler>();

            return collection.BuildServiceProvider();
        }
    }
}