namespace HTWDiscordBot.Services
{
    internal class HttpClientService
    {
        public readonly HttpClient httpClient;
        public readonly HttpClientHandler httpHandler;
        public HttpClientService()
        {
            httpHandler = new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false };
            httpClient = new HttpClient(httpHandler);
        }
    }
}
