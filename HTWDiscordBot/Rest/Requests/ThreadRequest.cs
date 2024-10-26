using HTWDiscordBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace HTWDiscordBot.Rest.Requests;

public record struct ThreadRequest
{
    [FromServices] public ConfigService ConfigService { get; init; }
    [FromServices] public IDbService DbService { get; init; }
    [FromRoute] public string ForumId { get; init; }
    [FromRoute] public string ThreadId { get; init; }
    [FromHeader] public string? Authorization { get; init; }
}