using HtmlAgilityPack;

namespace HTWDiscordBot.Services
{
    //Parsed die HTML-Datei in ein Scoreboard
    internal class HtmlParserService
    {
        public HtmlParserService()
        {
        }

        //Parsed die HTML-Datei in ein Scoreboard
        public string ParseScoreBoard(string html)
        {
            HtmlDocument document = LoadHtml(html);
            string scoreboard = "**" + String.Format("{0,-6} {1,-9} {2,-40}", "Platz", "Punktzahl", "Benutzername").Replace(" ", "᲼") + "**";

            foreach (HtmlNode player in document.DocumentNode.SelectSingleNode("/html/body/div/table/tbody").Descendants("tr").Take(25))
            {
                List<HtmlNode> playerData = player.Descendants("td").ToList();

                scoreboard += String.Format("\n{0,-5} {1,-9} {2,-40}", playerData[0].InnerText, playerData[2].InnerText, playerData[1].InnerText).Replace(" ", "᲼");
            }
            return scoreboard;
        }

        public string GetPlayerData(string html,string username)
        {
            HtmlDocument document = LoadHtml(html);
            string correctPlayerData = "**" + String.Format("{0,-6} {1,-9} {2,-40}", "Platz", "Punktzahl", "Benutzername").Replace(" ", "᲼") + "**";

            foreach (HtmlNode player in document.DocumentNode.SelectSingleNode("/html/body/div/table/tbody").Descendants("tr").Take(25))
            {
                List<HtmlNode> playerData = player.Descendants("td").ToList();

                if (playerData[1].InnerText == username)
                    correctPlayerData += String.Format("\n{0,-5} {1,-9} {2,-40}", playerData[0].InnerText, playerData[2].InnerText, playerData[1].InnerText).Replace(" ", "᲼");
            }

            return correctPlayerData;
        }

        //Lädt die HTML-Datei aus einem string
        private HtmlDocument LoadHtml(string html)
        {
            HtmlDocument document = new();
            document.LoadHtml(html);
            return document;
        }
    }
}