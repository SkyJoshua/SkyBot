using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class Ping : ICommand
    {
        public string Name => "ping";
        public string[] Aliases => [];
        public string Description => "Shows the bot's response time.";
        public string Section => "Info";
        public string Usage => "ping";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;
            
            DateTime start = DateTime.UtcNow;
            TaskResult<Message> message = await MessageHelper.ReplyAsync(ctx, channel, "🏓 Pinging...");
            double elapsed = (DateTime.UtcNow - start).TotalMilliseconds;

            await MessageHelper.EditAsync(channel, message.Data, $"🏓 Pong! `{elapsed:F0}ms`");
        }
    }
}