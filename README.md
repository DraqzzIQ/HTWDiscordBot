# HTWDiscordBot
### Discord Bot for the HTW Community Discord Server

Benachrichtigt wenn eine neue Aufgabe auf [Hack The Web](https://hack.arrrg.de/) online gestellt wurde

## Usage

`/stop` um zu deaktivieren

`/start` um zu reaktivieren

## Config

`.env` Datei im Stammverzeichnis:
```
htw-username //Zeile 1
htw-password //Zeile 2
bot-token    //Zeile 3
```

`challengeID.txt` Datei im Stammverzeichnis:
```
ID //z.B. 69
```
## How it works
Jede 30 Sekunden wird eine Anfrage an `https://hack.arrrg.de/challenge/{ID}` gesendet. Wenn die Seite existiert, wird der Statuscode OK(200) zurückgegeben. Andernfalls wird Redirect(302) zurückgegeben, da man auf die Startseite weitergeleited wird.
