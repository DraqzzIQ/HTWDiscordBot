using Discord.Interactions;

namespace HTWDiscordBot.Modules
{
    //WIP
    //Muss public sein, um vom InteractionHandler erkannt zu werden
    public class ModalModule : InteractionModuleBase<SocketInteractionContext>
    {
        [ModalInteraction("login")]
        public async Task VerifyLoginInfo(LoginModal modal)
        {
            await RespondAsync("Login successful", ephemeral: true);
        }
    }

    public class LoginModal : IModal
    {
        public string Title => "Login";

        [RequiredInput(true)]
        [InputLabel("Username")]
        [ModalTextInput("username", placeholder: "demo", minLength: 3)]
        public string Username { get; set; } = "";

        [RequiredInput(true)]
        [InputLabel("Password")]
        [ModalTextInput("password", placeholder: "1234", minLength: 4)]
        public string Password { get; set; } = "";
    }

}
