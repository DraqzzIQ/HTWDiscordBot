# HTWDiscordBot

### Discord Bot for the HTW Community Discord Server

Benachrichtigt wenn eine neue Aufgabe auf [Hack The Web](https://hack.arrrg.de/) online gestellt wird und zeigt das aktuelle Scoreboard an.

## Usage

`/login` um seinen HTW Account mit seinem Discord Account zu verknüpfen.
Dadurch wird sein aktueller Rank neben seinem Namen angezeigt.

`/logout` um die Verknüpfung aufzuheben

`/player` `<username>` um den Scoreboard-Eintrag eines Spielers anzuzeigen

## Config

`config.json` Datei im Stammverzeichnis:

```
{
  "Token": "",
  "ScoreboardChannelID": 0,
  "ChallengeChannelID": 0,
  "RoleChannelID": 0,
  "RoleID": 0,
  "ServerID": 0
}
```