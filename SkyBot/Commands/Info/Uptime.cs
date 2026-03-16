using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class Uptime : ICommand
    {
        public string Name => "uptime";
        public string[] Aliases => ["up"];
        public string Description => "Shows how long the bot has been running.";
        public string Section => "Info";
        public string Usage => "uptime";
    

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;
            
            TimeSpan uptime = DateTime.UtcNow - SkyBot.StartTime;
            string formatted = $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
            await MessageHelper.ReplyAsync(ctx, channel, $"⏱️ Uptime: `{formatted}`");
        }
    }
}