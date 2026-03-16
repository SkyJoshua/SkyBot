using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Source : ICommand
    {
        public string Name => "source";
        public string[] Aliases => ["src"];
        public string Description => "Shows the source code for this bot.";
        public string Section => "Info";
        public string Usage => "source";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;

            string message = $"You can find my source code here: {Config.SourceLink}";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}