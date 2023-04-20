using Discord.Interactions;
using HTWDiscordBot.Services;
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
            => await RespondWithModalAsync<LoginModal>("login");

        [SlashCommand("playerdata", "Zeigt den Platz auf dem Scoreboard und punkte eines Spielers an")]
        public async Task Playerdata([Summary(description: "Der Hack The Web Username")] string username)
        {
            if (await scoreboardService.GetPlayerdataAsync(username) != null)
                await Context.Interaction.RespondAsync(embed: await scoreboardService.GetPlayerdataAsync(username));
            else
                await Context.Interaction.RespondAsync("Username wurde nicht gefunden");
        }
    }
}