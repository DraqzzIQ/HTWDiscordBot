﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace HTWDiscordBot.Services
{
    internal class DiscordService
    {
        public readonly DiscordSocketClient client;
        private readonly ConfigService configService;
        private readonly DiscordSocketConfig discordSocketConfig;

        public DiscordService(DiscordSocketConfig discordSocketConfig, ConfigService configService)
        {
            this.discordSocketConfig = discordSocketConfig;
            this.configService = configService;

            ConfigureAsync();
            client = new(discordSocketConfig);
        }

        public async Task InitializeAsync()
        {
            client.Ready += Client_Ready;
            await client.LoginAsync(TokenType.Bot, configService.Config.Token);
            await client.StartAsync();
        }

        //Konfiguriert die DiscordSocketConfig
        private void ConfigureAsync()
        {
            discordSocketConfig.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds;
            discordSocketConfig.LogGatewayIntentWarnings = false;
        }

        //Wird ausgeführt, wenn der Bot bereit ist
        private async Task Client_Ready()
        {
            await ConfigureGlobalCommands();
            await client.SetGameAsync("Hack The Web", type: ActivityType.Playing);
        }

        //Erstellt die Slash Commands
        private async Task ConfigureGlobalCommands()
        {
            IReadOnlyCollection<SocketApplicationCommand> globalCommands = await client.GetGlobalApplicationCommandsAsync();

            SlashCommandBuilder startCommand = new();
            startCommand.WithName("start");
            startCommand.WithDescription("Startet das Tracking der Aufgaben");

            SlashCommandBuilder stopCommand = new();
            stopCommand.WithName("stop");
            stopCommand.WithDescription("Stoppt das Tracking der Aufgaben");

            try
            {
                //Wenn Slash Command nicht existiert, wird er erstellt
                if (!globalCommands.Any(x => x.Name == "start"))
                    await client.CreateGlobalApplicationCommandAsync(startCommand.Build());

                if (!globalCommands.Any(x => x.Name == "stop"))
                    await client.CreateGlobalApplicationCommandAsync(stopCommand.Build());
            }
            catch (HttpException exception)
            {
                // Wenn Command ungültig
                string json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
