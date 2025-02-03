using Discord.Interactions;
using Discord.WebSocket;
using HtwDiscordBot.Handlers;
using HtwDiscordBot.Services;
using HtwDiscordBot.Services.Htw;
using Microsoft.Extensions.DependencyInjection;

namespace HtwDiscordBot;

public class Program
{
    private readonly IServiceProvider services;
    private readonly DiscordSocketClient client;
    private readonly DiscordService discordService;
    private readonly LoggingService loggingService;
    private readonly HtwService htwService;
    private readonly InteractionHandler interactionHandler;
    private readonly HtwUserService userService;
    private readonly RoleService roleService;
    private bool initialized = false;

    private Program()
    {
        services = CreateProvider();
        client = services.GetRequiredService<DiscordSocketClient>();
        discordService = services.GetRequiredService<DiscordService>();
        loggingService = services.GetRequiredService<LoggingService>();
        htwService = services.GetRequiredService<HtwService>();
        interactionHandler = services.GetRequiredService<InteractionHandler>();
        userService = services.GetRequiredService<HtwUserService>();
        roleService = services.GetRequiredService<RoleService>();
    }

    public static Task Main() => new Program().MainAsync();

    private async Task MainAsync()
    {
        client.Ready += Client_ReadyAsync;
        client.Log += loggingService.LogAsync;

        await userService.InitializeAsync();
        await interactionHandler.InitializeAsync();
        await discordService.InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private async Task Client_ReadyAsync()
    {
        // don't run this method more than once
        if (initialized) return;

        initialized = true;

        await htwService.InitializeAsync();
        await roleService.InitializeAsync();
    }

    private static ServiceProvider CreateProvider()
    {
        IServiceCollection collection = new ServiceCollection()
            .AddSingleton<LoggingService>()
            .AddSingleton<ConfigService>()
            .AddSingleton(DiscordService.CreateDiscordSocketConfig())
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<DiscordService>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<HtwService>()
            .AddSingleton<ScoreboardService>()
            .AddSingleton<ChallengeService>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<HtwUserService>()
            .AddSingleton<RoleService>();

        collection.AddHttpClient("client", httpClient => { httpClient.BaseAddress = new Uri(Config.Url); })
            .ConfigurePrimaryHttpMessageHandler((c) =>
                new HttpClientHandler()
                {
                    UseCookies = false,
                    AllowAutoRedirect = false
                });

        return collection.BuildServiceProvider();
    }
}