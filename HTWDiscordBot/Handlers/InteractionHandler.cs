using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using HtwDiscordBot.Services;
using System.Reflection;

namespace HtwDiscordBot.Handlers;

public class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactionService,
    IServiceProvider serviceProvider,
    LoggingService loggingService)
{
    public async Task InitializeAsync()
    {
        client.Ready += ReadyAsync;
        interactionService.Log += loggingService.LogAsync;

        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

        client.InteractionCreated += HandleInteraction;
    }

    private async Task ReadyAsync()
    {
        await interactionService.RegisterCommandsGloballyAsync(true);
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            SocketInteractionContext context = new(client, interaction);

            IResult result = await interactionService.ExecuteCommandAsync(context, serviceProvider);

            if (!result.IsSuccess)
                await loggingService.LogAsync(new LogMessage(LogSeverity.Error, "HandleInteraction",
                    result.Error + " Reason: " + result.ErrorReason));
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}