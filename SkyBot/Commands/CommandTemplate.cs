using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
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
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;
            
        }
    }
}