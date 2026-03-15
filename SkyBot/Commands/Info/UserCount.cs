using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class UserCount : ICommand
    {
        public string Name => "usercount";
        public string[] Aliases => ["users"];
        public string Description => "Shows the user count of Valour.";
        public string Section => "Info";
        public string Usage => "usercount|users";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            
            string message = @$"Current Valour user count is: {ValourUsercountHelper.ValourUsercount:N0}
                                You can see a graph of the user count here: /meow";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}