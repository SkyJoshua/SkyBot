using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Test : ICommand
    {
        public string Name => "test";
        public string[] Aliases => [];
        public string Description => "Just a test command";
        public string Section => "Dev";
        public string Usage => "test";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            ValourClient client = ctx.Client;
            PlanetMember member = ctx.Member;
            Message message = ctx.Message;
            Planet planet = ctx.Planet;
        
            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "This is a test message");
            }
        }
    }
}