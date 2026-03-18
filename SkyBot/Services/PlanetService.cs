using System.Collections.Concurrent;
using Valour.Sdk.Client;
using Valour.Sdk.ModelLogic;
using Valour.Sdk.Models;

namespace SkyBot.Services
{
    public static class PlanetService
    {
        private static readonly DateTime _startTime = DateTime.UtcNow;
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

                    planet.Channels.Changed += async _ =>
                    {
                        await ChannelService.InitializeChannelsAsync(channelCache, planet);
                    };

                    planet.Members.Changed += async memberEvent =>
                    {
                        if ((DateTime.UtcNow - _startTime).TotalSeconds < 10) return;
                        if (memberEvent is ModelAddedEvent<PlanetMember> addedEvent)
                        {
                            await WelcomeService.OnMemberJoin(addedEvent.Model, channelCache);
                        }
                    };

                    initializedPlanets.TryAdd(planet.Id, true);
                });
            await Task.WhenAll(tasks);
        }
    }
}