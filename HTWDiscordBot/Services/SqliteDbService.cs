using HTWDiscordBot.Models;
using Microsoft.Data.Sqlite;

namespace HTWDiscordBot.Services;

public class SqliteDbService : IDbService
{
    private readonly SqliteConnection connection;

    public SqliteDbService()
    {
        // in-memory for now
        connection = new SqliteConnection("Data Source=forums.db;Mode=Memory;Cache=Shared;");
        connection.Open();

        // Create tables
        // ForumModel ( Id, Name, Threads )
        // ThreadModel ( Id, Name, Messages )
        // ThreadMessageModel ( Id, UserName, Content )

        using var cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Forum (Id INTEGER PRIMARY KEY, Name TEXT)", connection);
        cmd.ExecuteNonQuery();

        using var cmd2 =
            new SqliteCommand(
                "CREATE TABLE IF NOT EXISTS Thread (Id INTEGER PRIMARY KEY, Name TEXT, CreationDate TIME, ForumId INTEGER, FOREIGN KEY(ForumId) REFERENCES Forum(Id))",
                connection);
        cmd2.ExecuteNonQuery();

        using var cmd3 =
            new SqliteCommand(
                "CREATE TABLE IF NOT EXISTS ThreadMessage (Id INTEGER PRIMARY KEY, UserName TEXT, Content TEXT, CreationDate TIME, ThreadId INTEGER, FOREIGN KEY(ThreadId) REFERENCES Thread(Id))",
                connection);
        cmd3.ExecuteNonQuery();
    }

    public async Task CreateForumAsync(ForumModel forumModel)
    {
        await using var cmd = new SqliteCommand("INSERT INTO Forum (Id, Name) VALUES (@Id, @Name)", connection);
        cmd.Parameters.AddWithValue("@Id", forumModel.Id);
        cmd.Parameters.AddWithValue("@Name", forumModel.Name);
        await cmd.ExecuteNonQueryAsync();

        foreach (var threadModel in forumModel.Threads)
        {
            await CreateThreadAsync(forumModel.Id, threadModel);
        }
    }

    public async Task CreateThreadAsync(ulong forumId, ThreadModel threadModel)
    {
        await using var cmd =
            new SqliteCommand(
                "INSERT INTO Thread (Id, Name, CreationDate, ForumId) VALUES (@Id, @Name, @CreationDate, @ForumId)",
                connection);
        cmd.Parameters.AddWithValue("@Id", threadModel.Id);
        cmd.Parameters.AddWithValue("@Name", threadModel.Name);
        cmd.Parameters.AddWithValue("@CreationDate", threadModel.CreationDate);
        cmd.Parameters.AddWithValue("@ForumId", forumId);
        await cmd.ExecuteNonQueryAsync();

        foreach (var messageModel in threadModel.Messages)
        {
            await CreateThreadMessageAsync(threadModel.Id, messageModel);
        }
    }

    public async Task CreateThreadMessageAsync(ulong threadId, ThreadMessageModel messageModel)
    {
        await using var cmd =
            new SqliteCommand(
                "INSERT INTO ThreadMessage (Id, UserName, Content, CreationDate, ThreadId) VALUES (@Id, @UserName, @Content, @CreationDate, @ThreadId)",
                connection);
        cmd.Parameters.AddWithValue("@Id", messageModel.Id);
        cmd.Parameters.AddWithValue("@UserName", messageModel.UserName);
        cmd.Parameters.AddWithValue("@Content", messageModel.Content);
        cmd.Parameters.AddWithValue("@CreationDate", messageModel.CreationDate);
        cmd.Parameters.AddWithValue("@ThreadId", threadId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ThreadExistsAsync(ulong threadId)
    {
        await using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Thread WHERE Id = @Id", connection);
        cmd.Parameters.AddWithValue("@Id", threadId);
        return (long)await cmd.ExecuteScalarAsync() > 0;
    }

    public async Task DeleteThreadAsync(ulong threadId)
    {
        await using var cmd = new SqliteCommand("DELETE FROM ThreadMessage WHERE ThreadId = @ThreadId", connection);
        cmd.Parameters.AddWithValue("@ThreadId", threadId);
        await cmd.ExecuteNonQueryAsync();

        await using var cmd2 = new SqliteCommand("DELETE FROM Thread WHERE Id = @Id", connection);
        cmd2.Parameters.AddWithValue("@Id", threadId);
        await cmd2.ExecuteNonQueryAsync();
    }

    public async Task DeleteThreadMessageAsync(ulong threadMessageId)
    {
        await using var cmd = new SqliteCommand("DELETE FROM ThreadMessage WHERE Id = @Id", connection);
        cmd.Parameters.AddWithValue("@Id", threadMessageId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<ForumModel?> GetForumAsync(ulong forumId, bool includeThreads = true, bool includeMessages = true)
    {
        await using var cmd = new SqliteCommand("SELECT * FROM Forum WHERE Id = @Id", connection);
        cmd.Parameters.AddWithValue("@Id", forumId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        var forum = new ForumModel((ulong)reader.GetInt64(0), reader.GetString(1), []);
        if (!includeThreads) return forum;
        await using var cmd2 = new SqliteCommand("SELECT * FROM Thread WHERE ForumId = @ForumId", connection);
        cmd2.Parameters.AddWithValue("@ForumId", forumId);
        await using var reader2 = await cmd2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            var thread = new ThreadModel((ulong)reader2.GetInt64(0), reader2.GetString(1), reader2.GetDateTime(2), []);
            if (includeMessages)
            {
                await using var cmd3 =
                    new SqliteCommand("SELECT * FROM ThreadMessage WHERE ThreadId = @ThreadId", connection);
                cmd3.Parameters.AddWithValue("@ThreadId", thread.Id);
                await using var reader3 = await cmd3.ExecuteReaderAsync();
                while (await reader3.ReadAsync())
                {
                    thread.Messages.Add(new ThreadMessageModel((ulong)reader3.GetInt64(0), reader3.GetString(1),
                        reader3.GetString(2), reader3.GetDateTime(3)));
                }
            }

            forum.Threads.Add(thread);
        }

        return forum;
    }

    public async Task<ThreadModel?> GetThreadAsync(ulong threadId)
    {
        await using var cmd = new SqliteCommand("SELECT * FROM Thread WHERE Id = @Id", connection);
        cmd.Parameters.AddWithValue("@Id", threadId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        var thread = new ThreadModel((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetDateTime(2), []);
        await using var cmd2 = new SqliteCommand("SELECT * FROM ThreadMessage WHERE ThreadId = @ThreadId", connection);
        cmd2.Parameters.AddWithValue("@ThreadId", threadId);
        await using var reader2 = await cmd2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            thread.Messages.Add(new ThreadMessageModel((ulong)reader2.GetInt64(0), reader2.GetString(1),
                reader2.GetString(2), reader2.GetDateTime(3)));
        }

        return thread;
    }

    public async Task<ThreadMessageModel?> GetThreadMessageAsync(ulong threadMessageId)
    {
        await using var cmd = new SqliteCommand("SELECT * FROM ThreadMessage WHERE Id = @Id", connection);
        cmd.Parameters.AddWithValue("@Id", threadMessageId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new ThreadMessageModel((ulong)reader.GetInt64(0), reader.GetString(1), reader.GetString(2),
            reader.GetDateTime(3));
    }
}