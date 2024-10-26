using Discord;
using Discord.Rest;
using Discord.WebSocket;
using HTWDiscordBot.Models;

namespace HTWDiscordBot.Services.HTW
{
    public class ForumService
    {
        private readonly ConfigService configService;
        private readonly DiscordSocketClient client;
        private readonly LoggingService loggingService;
        private readonly IDbService dbService;
        private readonly Dictionary<ulong, (int Count, Dictionary<ulong, string> Names)> nickNames = new();

        public ForumService(ConfigService configService, DiscordSocketClient client, LoggingService loggingService,
            IDbService dbService)
        {
            this.configService = configService;
            this.client = client;
            this.loggingService = loggingService;
            this.dbService = dbService;
        }

        public async Task InitializeAsync()
        {
            // Don't block the gateway task
            Task.Run(GenerateMessageDb);

            client.MessageReceived += Client_MessageReceived;
            client.MessageDeleted += Client_MessageDeleted;
            client.MessageUpdated += Client_MessageUpdated;
            client.ThreadCreated += Client_ThreadCreated;
            client.ThreadUpdated += Client_ThreadUpdated;
            client.ThreadDeleted += Client_ThreadDeleted;
        }

        private async Task GenerateMessageDb()
        {
            ForumModel?[] forumModels =
            [
                await GetForumChannel(configService.Config.GermanForumChannelID),
                await GetForumChannel(configService.Config.EnglishForumChannelID)
            ];

            foreach (var forumModel in forumModels)
            {
                if (forumModel is not null)
                {
                    await dbService.CreateForumAsync(forumModel);
                }
            }
        }

        private async Task<ForumModel?> GetForumChannel(ulong forumChannelId)
        {
            var channel = await client.GetChannelAsync(forumChannelId) as SocketForumChannel;
            if (channel is null)
            {
                await loggingService.LogAsync(new LogMessage(LogSeverity.Error, "ForumService", "Channel not found"));
                return null;
            }

            var activeThreads = await channel.GetActiveThreadsAsync();
            var threads = activeThreads.ToList();

            var archivedThreads = (await channel.GetPublicArchivedThreadsAsync()).ToList();
            while (archivedThreads.Count > 0)
            {
                threads.AddRange(archivedThreads);
                archivedThreads =
                    (await channel.GetPublicArchivedThreadsAsync(before: archivedThreads.Last().ArchiveTimestamp))
                    .ToList();
            }

            threads.AddRange(archivedThreads);

            List<ThreadModel> threadModels = new();

            foreach (var thread in threads)
            {
                threadModels.Add(new ThreadModel(thread.Id, thread.Name, thread.CreatedAt.UtcDateTime,
                    await GetThreadMessages(thread)));
            }

            return new ForumModel(channel.Id, channel.Name, threadModels);
        }

        private async Task<List<ThreadMessageModel>> GetThreadMessages(RestThreadChannel thread)
        {
            var messages = await thread.GetMessagesAsync(limit: thread.MessageCount).FlattenAsync();
            messages = messages.Reverse();
            List<ThreadMessageModel> messageModels = new();
            if (!nickNames.ContainsKey(thread.Id)) nickNames.Add(thread.Id, (1, new Dictionary<ulong, string>()));
            foreach (var message in messages)
            {
                if(message.Author.IsBot) continue;
                
                if(message.Author.IsWebhook) continue;

                if (message.Type != MessageType.Default) continue;
                
                if (!nickNames[thread.Id].Names.ContainsKey(message.Author.Id))
                {
                    nickNames[thread.Id].Names.Add(message.Author.Id, "User " + nickNames[thread.Id].Count);
                    nickNames[thread.Id] = (nickNames[thread.Id].Count + 1, nickNames[thread.Id].Names);
                }

                messageModels.Add(new ThreadMessageModel(message.Id, nickNames[thread.Id].Names[message.Author.Id],
                    message.CleanContent,
                    message.CreatedAt.UtcDateTime));
            }

            return messageModels;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (!await dbService.ThreadExistsAsync(arg.Channel.Id))
                return;
            
            if (arg.Author.IsBot) return;
            
            if(arg.Author.IsWebhook) return;
            
            if(arg.Type != MessageType.Default) return;

            if (!nickNames.ContainsKey(arg.Channel.Id))
                nickNames.Add(arg.Channel.Id, (1, new Dictionary<ulong, string>()));

            if (!nickNames[arg.Channel.Id].Names.ContainsKey(arg.Author.Id))
            {
                nickNames[arg.Channel.Id].Names.Add(arg.Author.Id, "User " + nickNames[arg.Channel.Id].Count);
                nickNames[arg.Channel.Id] = (nickNames[arg.Channel.Id].Count + 1, nickNames[arg.Channel.Id].Names);
            }

            await dbService.CreateThreadMessageAsync(arg.Channel.Id,
                new ThreadMessageModel(arg.Id, nickNames[arg.Channel.Id].Names[arg.Author.Id], arg.Content,
                    arg.CreatedAt.UtcDateTime));
        }

        private async Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2,
            ISocketMessageChannel arg3)
        {
            if (!await dbService.ThreadExistsAsync(arg3.Id))
                return;

            if (arg2.Author.IsBot) return;
            
            if(arg2.Author.IsWebhook) return;
            
            if(arg2.Type != MessageType.Default) return;

            ThreadMessageModel threadMessageModel = (await dbService.GetThreadMessageAsync(arg2.Id))!;

            await dbService.DeleteThreadMessageAsync(arg2.Id);
            await dbService.CreateThreadMessageAsync(arg3.Id,
                new ThreadMessageModel(arg2.Id, threadMessageModel.UserName, arg2.Content, arg2.CreatedAt.UtcDateTime));
        }

        private async Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1,
            Cacheable<IMessageChannel, ulong> arg2)
        {
            if (await dbService.ThreadExistsAsync(arg2.Id))
                await dbService.DeleteThreadMessageAsync(arg1.Id);
        }

        private async Task Client_ThreadDeleted(Cacheable<SocketThreadChannel, ulong> arg)
        {
            if (await dbService.ThreadExistsAsync(arg.Id))
                await dbService.DeleteThreadAsync(arg.Id);
        }

        private async Task Client_ThreadUpdated(Cacheable<SocketThreadChannel, ulong> arg1, SocketThreadChannel arg2)
        {
            if (!await dbService.ThreadExistsAsync(arg1.Id))
                return;

            var thread = await dbService.GetThreadAsync(arg1.Id);
            List<ThreadMessageModel> messages = thread!.Messages;
            await dbService.DeleteThreadAsync(arg1.Id);
            await dbService.CreateThreadAsync(arg2.ParentChannel.Id,
                new ThreadModel(arg2.Id, arg2.Name, arg2.CreatedAt.UtcDateTime, messages));
        }

        private async Task Client_ThreadCreated(SocketThreadChannel arg)
        {
            if (configService.Config.EnglishForumChannelID != arg.ParentChannel.Id &&
                configService.Config.GermanForumChannelID != arg.ParentChannel.Id)
                return;

            if (await dbService.ThreadExistsAsync(arg.Id))
                return;

            await dbService.CreateThreadAsync(arg.ParentChannel.Id,
                new ThreadModel(arg.Id, arg.Name, arg.CreatedAt.UtcDateTime, []));
        }
    }
}