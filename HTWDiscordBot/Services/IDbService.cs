using HTWDiscordBot.Models;

namespace HTWDiscordBot.Services;

public interface IDbService
{
    public Task CreateForumAsync(ForumModel forumModel);
    public Task CreateThreadAsync(ulong forumId, ThreadModel threadModel);
    public Task CreateThreadMessageAsync(ulong threadId, ThreadMessageModel threadMessageModel);
    public Task<bool> ThreadExistsAsync(ulong threadId);
    public Task DeleteThreadAsync(ulong threadId);
    public Task DeleteThreadMessageAsync(ulong threadMessageId);
    public Task<ForumModel?> GetForumAsync(ulong forumId, bool includeThreads = true, bool includeMessages = true);
    public Task<ThreadModel?> GetThreadAsync(ulong threadId);
    public Task<ThreadMessageModel?> GetThreadMessageAsync(ulong threadMessageId);
}