using System.Collections.Concurrent;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class SwaggerAPI : ICommand
    {
        public string Name => "swagger";
        public string[] Aliases => ["api"];
        public string Description => "Sends a link to the Valour.gg Swagger API.";
        public string Section => "Info";
        public string Usage => "swagger|api";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            
            string message = $"Here is a link to the Swagger API: https://api.valour.gg/swagger";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}