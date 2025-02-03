using Discord;
using Discord.Interactions;
using HtwDiscordBot.Services.Htw;

namespace HtwDiscordBot.Modules;

public class SlashCommandModule(ScoreboardService scoreboardService, HtwUserService verifyUserService)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("login", "Logge dich ein, damit dein Rank neben deinem Benutzernamen angezeigt wird.")]
    public async Task Login()
    {
        ComponentBuilder componentBuilder = new ComponentBuilder()
            .WithButton(label: "Get Token", style: ButtonStyle.Link, url: "https://hack.arrrg.de/token")
            .WithButton(label: "Login", customId: "login-button", style: ButtonStyle.Primary, row: 1);

        await RespondAsync(
            "Öffne den Link, kopiere den Token und drücke auf **Login** um deinen Htw Account zu verknüpfen",
            components: componentBuilder.Build(), ephemeral: true);
    }

    [SlashCommand("logout", "Dein Rank wird nicht mehr neben deinem Benutzernamen angezeigt")]
    public async Task Logout()
    {
        await verifyUserService.LogoutAsync(Context.User.Id);
        await RespondAsync("Ausloggen erfolgreich.", ephemeral: true);
    }

    [SlashCommand("player", "Zeigt den Platz auf dem Scoreboard und punkte eines Spielers an")]
    public async Task Playerdata([Summary(description: "Der Hack The Web Username")] string username)
    {
        await DeferAsync();

        Embed? embed = await scoreboardService.GetPlayerdataAsync(username);

        if (embed != null)
            await Context.Interaction.FollowupAsync(embed: embed);
        else
            await Context.Interaction.FollowupAsync("Username wurde nicht gefunden");
    }
}