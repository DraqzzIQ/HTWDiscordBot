using HTWDiscordBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace HTWDiscordBot.Rest.Requests;

public record struct ThreadsRequest
{
    [FromServices] public ConfigService ConfigService { get; init; }
    [FromServices] public IDbService DbService { get; init; }
    [FromRoute] public string ForumId { get; init; }
    [FromHeader] public string? Authorization { get; init; }
}