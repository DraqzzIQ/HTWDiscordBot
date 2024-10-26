using HTWDiscordBot.Rest.Requests;
using HTWDiscordBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HTWDiscordBot.Rest.EndpointDefinitions;

public class ForumEndpointDefinitions : IEndpointDefinition
{
    public void DefineEndpoints(WebApplication app)
    {
        app.MapGet("/api/{forumId}/threads", GetThreads);
        app.MapGet("/api/{forumId}/thread/{threadId}", GetThread);
    }

    private async Task<IResult> GetThread([AsParameters] ThreadRequest request)
    {
        if (request.Authorization is null || request.Authorization != request.ConfigService.Config.AuthKey)
            return Results.Unauthorized();

        ulong forumId = GetForumId(request.ForumId, request.ConfigService);
        if (forumId == 0)
            return Results.NotFound("Forum not found");

        if (!ulong.TryParse(request.ThreadId, out ulong parsedThreadId))
            return Results.BadRequest("Invalid thread id");

        var thread = await request.DbService.GetThreadAsync(parsedThreadId);

        if (thread is null)
            return Results.NotFound("Thread not found");

        return Results.Ok(thread);
    }

    private async Task<IResult> GetThreads([AsParameters] ThreadsRequest request)
    {
        if (request.Authorization is null || request.Authorization != request.ConfigService.Config.AuthKey)
            return Results.Unauthorized();

        ulong forumId = GetForumId(request.ForumId, request.ConfigService);
        if (forumId == 0)
            return Results.NotFound("Forum not found");

        var forumModel = await request.DbService.GetForumAsync(forumId, includeMessages: false);

        if (forumModel is null)
            return Results.NotFound("Forum not found");

        return Results.Ok(forumModel);
    }

    private ulong GetForumId(string forum, ConfigService configService)
    {
        return forum switch
        {
            "ger" => configService.Config.GermanForumChannelID,
            "eng" => configService.Config.EnglishForumChannelID,
            _ => 0
        };
    }

    public void DefineServices(IServiceCollection services)
    {
    }
}