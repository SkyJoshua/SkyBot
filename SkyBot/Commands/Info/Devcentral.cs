using System.Collections.Concurrent;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Devcentral : ICommand
    {
        public string Name => "devcentral";
        public string[] Aliases => ["dev"];
        public string Description => "Sends an invite link to the Dev Central Planet.";
        public string Section => "Info";
        public string Usage => "devcentral|dev";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            
            string message = $"you can join the Dev Central (ID: 42439954653511681) planet here: https://app.valour.gg/I/k2tz9c4i";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}