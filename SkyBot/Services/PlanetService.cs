using System.Collections.Concurrent;
using SkyBot.Services;
using Valour.Sdk.Client;
using Valour.Sdk.Models;
using Valour.Sdk.Models.Messages.Embeds;


namespace SkyBot.Services
{
    public static class PlanetService
    {
        public static async Task InitializePlanetsAsync(
            ValourClient client,
            ConcurrentDictionary<long, Channel> channelCache,
            ConcurrentDictionary<long, bool> initializedPlanets)
        {
            var tasks = client.PlanetService.JoinedPlanets
                .Where(planet => !initializedPlanets.ContainsKey(planet.Id))
                .Select(async planet =>
                {
                    Console.WriteLine($"Initializing Planet: {planet.Name}");
                    await planet.EnsureReadyAsync();
                    await planet.FetchInitialDataAsync();
                    await ChannelService.InitializeChannelsAsync(channelCache, planet);

                    planet.Channels.Changed += async (channelEvent) => {
                        await ChannelService.InitializeChannelsAsync(channelCache, planet);
                    };
                });

            await Task.WhenAll(tasks);
        }
    }
}