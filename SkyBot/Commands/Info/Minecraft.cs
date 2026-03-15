using System.Collections.Concurrent;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Minecraft : ICommand
    {
        public string Name => "minecraft";
        public string[] Aliases => ["mc"];
        public string Description => "Sends the Unofficial ValourSMP IPs";
        public string Section => "Info";
        public string Usage => "minecraft|mc";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            
            string message = @$"you can join the Unofficial ValourSMP Minecraft Server by using this ip: 
                                Java: `valour.sxsc.xyz`, Bedrock: `valourbr.sxsc.xyz` Both with the default ports.
                                Cool features can be found here: https://sxsc.xyz/servers/valour/";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}