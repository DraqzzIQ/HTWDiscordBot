namespace HTWDiscordBot.Models;

public record ForumModel(ulong Id, string Name, List<ThreadModel> Threads);