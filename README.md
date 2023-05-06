# HTWDiscordBot

### Discord Bot for the HTW Community Discord Server

Benachrichtigt wenn eine neue Aufgabe auf [Hack The Web](https://hack.arrrg.de/) online gestellt wird und zeigt das aktuelle Scoreboard an.

## Usage

`/login` um seinen HTW Account mit seinem Discord Account zu verknüpfen.
Dadurch wird sein aktueller Rank neben seinem Namen angezeigt.

`/logout` um die Verknüpfung aufzuheben

`/player` `<username>` um den Scoreboard-Eintrag eines Spielers anzuzeigen

## Config

Als `username` kann `demo` und als `password` kann `1234` verwendet werden.

`config.json` Datei im Stammverzeichnis:

```
{
  "Username": "demo",
  "Password": "1234",
  "Token": "",
  "ScoreboardChannelID": 0,
  "ChallengeChannelID": 0,
  "RoleChannelID": 0,
  "RoleID": 0,
  "ServerID": 0
}
```

`challengeID.txt` Datei im Stammverzeichnis:

```
ID //z.B. 69
```

## How it works

Jede 30 Sekunden wird eine Anfrage an `https://hack.arrrg.de/challenge/{ID}` gesendet. Wenn die Seite existiert, wird der Statuscode `OK(200)` zurückgegeben. Andernfalls wird `Redirect(302)` zurückgegeben, da man auf die Startseite weitergeleited wird.