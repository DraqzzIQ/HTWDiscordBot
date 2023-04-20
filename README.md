# HTWDiscordBot

### Discord Bot for the HTW Community Discord Server

Benachrichtigt wenn eine neue Aufgabe auf [Hack The Web](https://hack.arrrg.de/) online gestellt wird und zeigt das aktuelle Scoreboard an.

## Usage

`/login` um seinen HTW Account mit seinem Discord Account zu verknüpfen.
Dadurch wird sein aktueller Rank neben seinem Namen angezeigt.
Es werden **keine** Anmeldedaten gespeichert

## Config

Als `username` kann `demo` und als `password` kann `1234` verwendet werden.

`.env` Datei im Stammverzeichnis:

```
htw-username           //Zeile 1
htw-password           //Zeile 2
bot-token              //Zeile 3
scoreboard-channel-id  //Zeile 4
challenge-channel-id   //Zeile 5
server-id              //Zeile 6
```

`challengeID.txt` Datei im Stammverzeichnis:

```
ID //z.B. 69
```

## How it works

Jede 30 Sekunden wird eine Anfrage an `https://hack.arrrg.de/challenge/{ID}` gesendet. Wenn die Seite existiert, wird der Statuscode `OK(200)` zurückgegeben. Andernfalls wird `Redirect(302)` zurückgegeben, da man auf die Startseite weitergeleited wird.