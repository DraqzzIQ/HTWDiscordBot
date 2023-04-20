using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HTWDiscordBot.Services;
using System.Reflection;

namespace HTWDiscordBot.Handlers
{
    //Handelt alle Art von Interaktionen
    public class InteractionHandler
    {
        private readonly DiscordSocketClient client;
        private readonly InteractionService interactionService;
        private readonly IServiceProvider serviceProvider;
        private readonly LoggingService loggingService;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider, LoggingService loggingService)
        {
            this.client = client;
            this.interactionService = interactionService;
            this.serviceProvider = serviceProvider;
            this.loggingService = loggingService;
        }

        public async Task InitializeAsync()
        {
            client.Ready += ReadyAsync;
            interactionService.Log += loggingService.LogAsync;

            //Fügt alle Module aus diesem Assembly hinzu
            await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

            client.InteractionCreated += HandleInteraction;
        }

        private async Task ReadyAsync()
        {
            //Regestriert alle Commands global
            await interactionService.RegisterCommandsGloballyAsync(true);
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                //Erstellt einen neuen InteractionContext indem der Command ausgeführt wird
                SocketInteractionContext context = new(client, interaction);

                IResult result = await interactionService.ExecuteCommandAsync(context, serviceProvider);

                if (!result.IsSuccess)
                    await loggingService.LogAsync(new LogMessage(LogSeverity.Error, "HandleInteraction", result.Error + " Reason: " + result.ErrorReason));
            }
            catch
            {
                //Wenn es einen Fehler gibt, wird der Command gelöscht
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
