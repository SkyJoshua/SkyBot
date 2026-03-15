using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace SkyBot.Services
{
    public static class ChannelService
    {
        public static async Task InitializeChannelsAsync(
            ConcurrentDictionary<long, Channel> channelCache,
            Planet planet)
        {
            foreach (var channel in planet.Channels)
            {
                channelCache[channel.Id] = channel;
            }

            _ = Task.Run(async () =>
            {
                foreach (var channel in planet.Channels.Where(c => c.ChannelType == ChannelTypeEnum.PlanetChat)){
                    try
                    {
                        await channel.OpenWithResult("SkyBot");
                        Console.WriteLine($"Realtime opened for: {planet.Name} (ID: {planet.Id}) -> {channel.Name} (ID: {channel.Id})");
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"Error opening realtime for {channel.Id}: {ex.Message}");
                    }
                }

                Console.WriteLine($"All channels opened for {planet.Name}.");
            });
        }
    }
}