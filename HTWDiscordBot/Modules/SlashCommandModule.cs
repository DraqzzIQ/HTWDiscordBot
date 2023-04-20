using Discord.Interactions;
using HTWDiscordBot.Services;

namespace HTWDiscordBot.Modules
{
    //Muss public sein, um vom InteractionHandler erkannt zu werden
    public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly HTWService hTWService;

        public SlashCommandModule(HTWService hTWService)
        {
            this.hTWService = hTWService;
        }

        //Gibt ein Modal zurück mit dem man sich einloggen kann
        [SlashCommand("login", "Logge dich ein, damit dein Rank angezeigt wird. Keine Anmeldedaten werden gespeichert.")]
        public async Task Login()
            => await RespondWithModalAsync<LoginModal>("login");
    }
}