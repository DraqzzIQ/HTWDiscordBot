using Discord.Interactions;
using Discord.WebSocket;
using HTWDiscordBot.Extensions;
using HTWDiscordBot.Handlers;
using HTWDiscordBot.Services;
using HTWDiscordBot.Services.HTW;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HTWDiscordBot
{
    public class Program
    {
        private readonly DiscordSocketClient client;
        private readonly DiscordService discordService;
        private readonly LoggingService loggingService;
        private readonly HTWService htwService;
        private readonly InteractionHandler interactionHandler;
        private readonly HTWUserService userService;
        private readonly RoleService roleService;
        private readonly ForumService forumService;
        private readonly WebApplication app;
        private bool initialized = false;

        private Program()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://*:5050");
            builder.Services.AddEndpointDefinitions(typeof(Program));
            CreateProvider(builder.Services);

            IServiceProvider services = builder.Services.BuildServiceProvider();
            client = services.GetRequiredService<DiscordSocketClient>();
            discordService = services.GetRequiredService<DiscordService>();
            loggingService = services.GetRequiredService<LoggingService>();
            htwService = services.GetRequiredService<HTWService>();
            interactionHandler = services.GetRequiredService<InteractionHandler>();
            userService = services.GetRequiredService<HTWUserService>();
            roleService = services.GetRequiredService<RoleService>();
            forumService = services.GetRequiredService<ForumService>();

            app = builder.Build();
            app.UseHttpsRedirection();
            app.UseEndpointDefinitions();
        }

        public static Task Main() => new Program().MainAsync();

        private async Task MainAsync()
        {
            client.Ready += Client_ReadyAsync;
            client.Log += loggingService.LogAsync;

            await userService.InitializeAsync();
            await interactionHandler.InitializeAsync();
            await discordService.InitializeAsync();

            await app.RunAsync();
        }

        //Wird ausgeführt, wenn der Bot bereit ist
        private async Task Client_ReadyAsync()
        {
            //Verhindert das die Loops mehrfach ausgeführt werden, wenn discord reconnected
            if (initialized) return;
            initialized = true;

            await htwService.InitializeAsync();
            await roleService.InitializeAsync();
            await forumService.InitializeAsync();
        }

        //ServiceProvider für Dependency Injection
        private static void CreateProvider(IServiceCollection services)
        {
            services
                .AddSingleton<LoggingService>()
                .AddSingleton<ConfigService>()
                .AddSingleton(DiscordService.CreateDiscordSockteConfig())
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<DiscordService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<HTWService>()
                .AddSingleton<ScoreboardService>()
                .AddSingleton<ChallengeService>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton<HTWUserService>()
                .AddSingleton<RoleService>()
                .AddSingleton<ForumService>()
                .AddSingleton<IDbService, SqliteDbService>();
            services.AddHttpClient("client", httpClient => { httpClient.BaseAddress = new Uri(Config.Url); })
                .ConfigurePrimaryHttpMessageHandler((c) =>
                    new HttpClientHandler()
                    {
                        UseCookies = false,
                        AllowAutoRedirect = false
                    });
        }
    }
}