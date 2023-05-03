using Discord;
using Discord.Interactions;
using HTWDiscordBot.Services.HTW;

namespace HTWDiscordBot.Modules
{
    //Muss public sein, um vom InteractionHandler erkannt zu werden
    public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ScoreboardService scoreboardService;

        public SlashCommandModule(ScoreboardService scoreboardService)
        {
            this.scoreboardService = scoreboardService;
        }

        //Gibt ein Modal zurück mit dem man sich einloggen kann
        [SlashCommand("login", "Logge dich ein, damit dein Rank angezeigt wird. Keine Anmeldedaten werden gespeichert.")]
        public async Task Login()
        {
            ComponentBuilder componentBuilder = new ComponentBuilder()
                .WithButton(label: "Get Token", style: ButtonStyle.Link, url: "https://hack.arrrg.de/token")
                .WithButton(label: "Login", customId: "login-button", style: ButtonStyle.Primary, row: 1);

            await RespondAsync("Öffne den Link, kopiere den Token und drücke auf **Login** um deinen HTW Account zu verknüpfen", components: componentBuilder.Build(), ephemeral: true);
        }

        [SlashCommand("player", "Zeigt den Platz auf dem Scoreboard und punkte eines Spielers an")]
        public async Task Playerdata([Summary(description: "Der Hack The Web Username")] string username)
        {
            await DeferAsync();

            if (await scoreboardService.GetPlayerdataAsync(username) != null)
                await Context.Interaction.FollowupAsync(embed: await scoreboardService.GetPlayerdataAsync(username));
            else
                await Context.Interaction.FollowupAsync("Username wurde nicht gefunden");
        }
    }
}