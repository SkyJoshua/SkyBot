using System.Collections.Concurrent;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace SkyBot.Services
{
    public static class ChannelService
    {
        private static readonly SemaphoreSlim _channelSemaphore = new SemaphoreSlim(3, 3);
        public static async Task InitializeChannelsAsync(
            ConcurrentDictionary<long, Channel> channelCache,
            Planet planet)
        {
            var tasks = planet.Channels.Select(async channel =>
            {
                channelCache[channel.Id] = channel;
                if (channel.ChannelType == ChannelTypeEnum.PlanetChat)
                {
                    await _channelSemaphore.WaitAsync();
                    try
                    {
                        await channel.OpenWithResult("SkyBot");
                        Console.WriteLine($"Realtime opened for: {planet.Name} (ID: {planet.Id}) -> {channel.Name} (ID: {channel.Id})");
                        await Task.Delay(250);
                    }
                    finally
                    {
                        _channelSemaphore.Release();
                    }
                    
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}