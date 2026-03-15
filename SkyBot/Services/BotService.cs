using System.Collections.Concurrent;
using DotNetEnv;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Services
{
    public static class BotService
    {
        public static async Task InitializeBotAsync(
            ValourClient client,
            ConcurrentDictionary<long, Channel> channelCache,
            ConcurrentDictionary<long, bool> initalizedPlanets)
        {
            Env.Load();

            var token = Environment.GetEnvironmentVariable("TOKEN");
            if (string.IsNullOrWhiteSpace(token)) {Console.WriteLine("TOKEN not set."); return;}

            var loginResult = await client.InitializeUser(token);
            if (!loginResult.Success) {Console.WriteLine($"Login Failed: {loginResult.Message}"); return;}
            Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

            await PlanetService.InitializePlanetsAsync(client, channelCache, initalizedPlanets);
            client.PlanetService.JoinedPlanetsUpdated += async () =>
            {
                await PlanetService.InitializePlanetsAsync(client, channelCache, initalizedPlanets);
            };

            client.MessageService.MessageReceived += async (message) =>
            {
                await Messages.Create.MessageAsync(client, channelCache, message);
            };
        }
    }
}