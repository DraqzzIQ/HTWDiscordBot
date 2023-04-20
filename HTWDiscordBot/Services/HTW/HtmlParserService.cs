using HtmlAgilityPack;

namespace HTWDiscordBot.Services
{
    //Parsed die HTML-Datei in ein Scoreboard
    public class HtmlParserService
    {
        private readonly string scoreboardPath = "/html/body/div/table/tbody";
        private readonly string usernamePath = "/html/body/div[1]/p/strong[1]";

        public HtmlParserService()
        {
        }

        //Parsed die HTML-Datei in ein Scoreboard
        public string ParseScoreBoard(string html)
        {
            HtmlDocument document = LoadHtml(html);
            string scoreboard = "**" + String.Format("{0,-6} {1,-9} {2,-40}", "Platz", "Punktzahl", "Benutzername").Replace(" ", "᲼") + "**";

            foreach (HtmlNode player in document.DocumentNode.SelectSingleNode(scoreboardPath).Descendants("tr").Take(50))
            {
                List<HtmlNode> playerData = player.Descendants("td").ToList();

                scoreboard += String.Format("\n{0,-5} {1,-9} {2,-40}", playerData[0].InnerText, playerData[2].InnerText, playerData[1].InnerText).Replace(" ", "᲼");
            }
            return scoreboard;
        }

        //Versucht einen Username aus der HTML-Datei zu parsen
        public string? ParseUsername(string html)
        {
            HtmlDocument document = LoadHtml(html);

            HtmlNode username = document.DocumentNode.SelectSingleNode(usernamePath);

            if (username == null)
                return null;

            return username.InnerText;
        }

        //Extrahiert einen bestimmten Eintrag aus dem Scoreboard
        public List<HtmlNode>? GetScoreBoardEntry(string html, string username)
        {
            HtmlDocument document = LoadHtml(html);

            foreach (HtmlNode player in document.DocumentNode.SelectSingleNode(scoreboardPath).Descendants("tr"))
            {
                List<HtmlNode> entry = player.Descendants("td").ToList();

                if (entry[1].InnerText == username)
                {
                    return entry;
                }
            }

            return null;
        }

        //Lädt die HTML-Datei aus einem string
        private static HtmlDocument LoadHtml(string html)
        {
            HtmlDocument document = new();
            document.LoadHtml(html);
            return document;
        }
    }
}