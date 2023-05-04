using Discord;
using Discord.WebSocket;

namespace HTWDiscordBot.Services.HTW
{
    public class RoleService
    {
        private readonly ConfigService configService;
        private readonly DiscordSocketClient client;
        private readonly LoggingService loggingService;
        private readonly string emoji = "✅";

        public RoleService(ConfigService configService, DiscordSocketClient client, LoggingService loggingService)
        {
            this.configService = configService;
            this.client = client;
            this.loggingService = loggingService;
        }

        public async Task InitializeAsync()
        {
            client.ReactionAdded += Client_ReactionAdded;
            client.ReactionRemoved += Client_ReactionRemoved;

            SocketTextChannel? textChannel = await client.GetChannelAsync(configService.Config.RoleChannelID) as SocketTextChannel;

            if (textChannel == null)
            {
                await loggingService.LogAsync(new(LogSeverity.Error, "RoleService", "TextChannel konnte nicht gefunden werden!"));
                return;
            }

            IMessage? message = textChannel.GetMessagesAsync(1).FlattenAsync().Result.FirstOrDefault();


            if (message != null && message.Author.Id == client.CurrentUser.Id)
                return;

            message = await textChannel.SendMessageAsync("Reagiere unten um benachrichtigt zu werden wenn eine neue Aufgabe verfügbar ist.");
            await message.AddReactionAsync(new Emoji(emoji));
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            SocketTextChannel? channel = cachedChannel.GetOrDownloadAsync().Result as SocketTextChannel;

            if (reaction.Emote.Name != emoji)
                return;
            if (channel?.Id != configService.Config.RoleChannelID)
                return;

            SocketGuildUser? user = channel?.Guild.GetUser(reaction.UserId);
            if (user == null)
                return;

            SocketRole? role = channel?.Guild.GetRole(configService.Config.RoleID);
            if (role == null)
            {
                loggingService.Log(new(LogSeverity.Error, "RoleService", "Rolle konnte nicht gefunden werden!"));
                return;
            }

            await user.AddRoleAsync(role);
        }

        private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            SocketTextChannel? channel = cachedChannel.GetOrDownloadAsync().Result as SocketTextChannel;

            if (reaction.Emote.Name != emoji)
                return;
            if (channel?.Id != configService.Config.RoleChannelID)
                return;

            SocketGuildUser? user = channel?.Guild.GetUser(reaction.UserId);
            if (user == null)
                return;

            SocketRole? role = channel?.Guild.GetRole(configService.Config.RoleID);
            if (role == null)
            {
                loggingService.Log(new(LogSeverity.Error, "RoleService", "Rolle konnte nicht gefunden werden!"));
                return;
            }

            await user.RemoveRoleAsync(role);
        }
    }
}