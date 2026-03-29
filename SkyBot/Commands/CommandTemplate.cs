using System.Collections.Concurrent;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class CommandTemplate : ICommand
    {
        public string Name => "template";
        public string[] Aliases => [];
        public string Description => "";
        public string Section => "template";
        public string Usage => "";

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
            
        }
    }
}