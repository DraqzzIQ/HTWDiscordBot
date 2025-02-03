using Discord.Interactions;
using HtwDiscordBot.Services.Htw;

namespace HtwDiscordBot.Modules;

public class InteractionModule(HtwUserService verifyUserService) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("login-button")]
    public async Task SendLoginModal()
        => await RespondWithModalAsync<LoginModal>("login-model");

    [ModalInteraction("login-model")]
    public async Task VerifyLoginInfo(LoginModal modal)
    {
        await DeferAsync();
        await FollowupAsync(await verifyUserService.IsRealUserAsync(modal.Username, modal.Token, Context.User.Id),
            ephemeral: true);
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
    [InputLabel("Token")]
    [ModalTextInput("token",
        placeholder: "1663121645431%7C%243a%2410%2465gQfqQK7gjnizf8upUwxOZWpYUD1hpz%2Fi7pbyAol1bMzHpCuAGka")]
    public string Token { get; set; } = "";
}