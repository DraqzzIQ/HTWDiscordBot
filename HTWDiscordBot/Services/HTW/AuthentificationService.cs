using System.Text.RegularExpressions;

namespace HTWDiscordBot.Services.HTW
{
    //Authentifierungs Logik
    public class AuthentificationService
    {
        private readonly HttpClientService httpService;

        public AuthentificationService(HttpClientService httpService)
        {
            this.httpService = httpService;
        }

        //Authentifiziert sich mit konfiguriertem Nutzername und Passwort und gibt den Session Cookie zurück
        public async Task<string> GetAuthCookieAsync(Dictionary<string, string> requestContent)
        {
            string authCookie = "";

            //HttpRequestMessage um den session id cookie zu bekommen
            HttpRequestMessage requestMessage = new(HttpMethod.Post, "login");

            //Fügt die login Daten hinzu
            requestMessage.Content = new FormUrlEncodedContent(requestContent);
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            //Liest den Cookie mit der session id aus
            foreach (string cookie in responseMessage.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value)
                if (cookie.Contains("connect.sid"))
                    authCookie = ExtractSessionId(cookie);
            return authCookie;
        }

        //Nutzt Regex um den Cookie zu extrahieren
        private static string ExtractSessionId(string cookieString)
        {
            Regex regex = new(@"connect\.sid=([^;]+);");
            Match match = regex.Match(cookieString);
            return match.Groups[1].Value;
        }
    }
}