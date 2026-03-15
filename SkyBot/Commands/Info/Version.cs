using System.Collections.Concurrent;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Version : ICommand
    {
        public string Name => "version";
        public string[] Aliases => [];
        public string Description => "Shows the current version of the Bot and Valour.";
        public string Section => "Info";
        public string Usage => "";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            
            string message = @$"Bot Version: {typeof(Version).Assembly.GetName().Version}
                                Valour Version: {typeof(Channel).Assembly.GetName().Version}";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}