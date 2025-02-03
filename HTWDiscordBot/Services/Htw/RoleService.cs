using Discord;
using Discord.WebSocket;

namespace HtwDiscordBot.Services.Htw;

public class RoleService(ConfigService configService, DiscordSocketClient client, LoggingService loggingService)
{
    private const string emoji = "✅";

    private const string activeHackerText =
        "Reagiere unten, um benachrichtigt zu werden, wenn eine neue Aufgabe verfügbar ist.";

    private const string ctfText = "Reagiere unten, um über CTFs benachrichtigt zu werden.";

    public async Task InitializeAsync()
    {
        client.ReactionAdded += Client_ReactionAdded;
        client.ReactionRemoved += Client_ReactionRemoved;

        SocketTextChannel? textChannel =
            await client.GetChannelAsync(configService.Config.RolesChannelID) as SocketTextChannel;

        if (textChannel == null)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "RoleService",
                "TextChannel konnte nicht gefunden werden!"));
            return;
        }

        // active hacker
        if (configService.Config.ActiveHackerMessageID != 0)
        {
            IMessage message = await textChannel.GetMessageAsync(configService.Config.ActiveHackerMessageID);
            if (message.Content != activeHackerText)
                await textChannel.ModifyMessageAsync(message.Id, m => m.Content = activeHackerText);
        }
        else
        {
            IMessage message = await textChannel.SendMessageAsync(activeHackerText);
            await message.AddReactionAsync(new Emoji(emoji));
            configService.Config.ActiveHackerMessageID = message.Id;
            configService.SaveConfig();
        }

        // ctf
        if (configService.Config.CTFMessageID != 0)
        {
            IMessage message = await textChannel.GetMessageAsync(configService.Config.CTFMessageID);
            if (message.Content != ctfText)
                await textChannel.ModifyMessageAsync(message.Id, m => m.Content = ctfText);
        }
        else
        {
            IMessage message = await textChannel.SendMessageAsync(ctfText);
            await message.AddReactionAsync(new Emoji(emoji));
            configService.Config.CTFMessageID = message.Id;
            configService.SaveConfig();
        }
    }

    private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        SocketTextChannel? channel = cachedChannel.GetOrDownloadAsync().Result as SocketTextChannel;

        if (reaction.Emote.Name != emoji)
            return;
        if (channel?.Id != configService.Config.RolesChannelID)
            return;

        SocketGuildUser? user = channel?.Guild.GetUser(reaction.UserId);
        if (user == null)
            return;

        ulong messageID = cachedMessage.Id;
        ulong roleID = messageID == configService.Config.ActiveHackerMessageID
            ? configService.Config.ActiveHackerRoleID
            : configService.Config.CTFRoleID;

        SocketRole? role = channel?.Guild.GetRole(roleID);
        if (role == null)
        {
            loggingService.Log(new(LogSeverity.Error, "RoleService",
                $"Rolle {roleID} konnte nicht gefunden werden!"));
            return;
        }

        await user.AddRoleAsync(role);
    }

    private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage,
        Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        SocketTextChannel? channel = cachedChannel.GetOrDownloadAsync().Result as SocketTextChannel;

        if (reaction.Emote.Name != emoji)
            return;
        if (channel?.Id != configService.Config.RolesChannelID)
            return;

        SocketGuildUser? user = channel?.Guild.GetUser(reaction.UserId);
        if (user == null)
            return;

        ulong messageID = cachedMessage.Id;
        ulong roleID = messageID == configService.Config.ActiveHackerMessageID
            ? configService.Config.ActiveHackerRoleID
            : configService.Config.CTFRoleID;

        SocketRole? role = channel?.Guild.GetRole(roleID);
        if (role == null)
        {
            loggingService.Log(new(LogSeverity.Error, "RoleService",
                $"Rolle {roleID} konnte nicht gefunden werden!"));
            return;
        }

        await user.RemoveRoleAsync(role);
    }
}