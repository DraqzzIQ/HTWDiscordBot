using Discord.Interactions;

namespace HTWDiscordBot.modules
{
    //Muss public sein, um vom InteractionHandler erkannt zu werden
    public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("start", "Aktiviert den Bot")]
        public async Task Start()
            => await RespondAsync("Started");

        [SlashCommand("stop", "Deaktiviert den Bot")]
        public async Task Stop()
            => await RespondAsync("Stopped");

        [SlashCommand("login", "Logge dich ein, damit dein Rank angezeigt wird. Keine Anmeldedaten werden gespeichert.")]
        public async Task Login()
        {
            //Gibt ein Modal zurück mit dem man sich einloggen kann
            await Context.Interaction.RespondWithModalAsync<LoginModal>("login");
        }
    }
}