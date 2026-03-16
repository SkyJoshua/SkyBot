using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class JoinSite : ICommand
    {
        public string Name => "joinsite";
        public string[] Aliases => [];
        public string Description => "Links to a site to help your bots join a planet.";
        public string Section => "Info";
        public string Usage => "joinsite";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            
            string message = $"You can use this website to easily add your bot to a planet: https://skyjoshua.xyz/planetjoiner";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}