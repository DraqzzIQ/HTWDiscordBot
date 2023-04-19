namespace HTWDiscordBot.Services
{
    //Stellt einen HttpClient bereit
    internal class HttpClientService
    {
        public readonly HttpClient httpClient;
        public readonly HttpClientHandler httpHandler;
        private readonly Uri url;

        public HttpClientService(ConfigService configService)
        {
            url = new(configService.Config.Url);
            httpHandler = new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false };
            httpClient = new HttpClient(httpHandler) { BaseAddress = url };
        }
    }
}
