using Discord.WebSocket;
using HTWDiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HTWDiscordBot
{
    public class Program
    {
        private readonly IServiceProvider services;
        private readonly DiscordService discordService;
        private readonly LoggingService loggingService;
        private readonly SlashCommandService slashCommandService;
        private readonly HTWService htwService;

        public Program()
        {
            services = CreateProvider();
            discordService = services.GetRequiredService<DiscordService>();
            loggingService = services.GetRequiredService<LoggingService>();
            slashCommandService = services.GetRequiredService<SlashCommandService>();
            htwService = services.GetRequiredService<HTWService>();
        }

        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            await discordService.InitializeAsync();
            await loggingService.InitializeAsync();
            await slashCommandService.InitializeAsync();
            await htwService.InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        //ServiceProvider für Dependency Injection
        private static IServiceProvider CreateProvider()
        {
            IServiceCollection collection = new ServiceCollection()
                .AddSingleton<DiscordService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<ConfigService>()
                .AddSingleton<SlashCommandService>()
                .AddSingleton<HTWService>()
                .AddSingleton<DiscordSocketConfig>()
                .AddSingleton<HttpClientService>();

            return collection.BuildServiceProvider();
        }
    }
}