using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class BotGuide : ICommand
    {
        public string Name => "botguide";
        public string[] Aliases => ["bot", "bguide"];
        public string Description => "Sends a link the a bot guide that SkyJoshua has made.";
        public string Section => "Info";
        public string Usage => "botguide";

        public async Task Execute(CommandContext ctx)
        {
        ValourClient Client = ctx.Client;
        ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
        PlanetMember Member = ctx.Member;
        Message Message = ctx.Message;
        Planet Planet = ctx.Planet;
        long channelId = ctx.ChannelId;
        string[] Args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;
            
            string msg = @"Here is a link to a Bot Guide that SkyJoshua has made (WIP):
                        https://github.com/SkyJoshua/Valour-Bot-Guide";

            await MessageHelper.ReplyAsync(ctx, channel, msg);
        }
    }
}