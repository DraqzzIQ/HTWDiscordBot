namespace HTWDiscordBot.Models;

public record ThreadModel(ulong Id, string Name, DateTime CreationDate, List<ThreadMessageModel> Messages);