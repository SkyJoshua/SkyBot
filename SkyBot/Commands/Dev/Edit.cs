using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class Edit : ICommand
    {
        public string Name => "edit";
        public string[] Aliases => [];
        public string Description => "Edit the bots message";
        public string Section => "Dev";
        public string Usage => "reply -> edit <message>";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            ValourClient client = ctx.Client;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            Message message = ctx.Message;
            string[] args = ctx.Args;
        
            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if(!PermissionHelper.IsOwner(member))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "This is a Dev only command.");
                    return;
                }

                if (message.ReplyToId == null)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please reply to a message.");
                    return;
                }

                if (client.Cache.Messages.TryGet(message.ReplyToId.Value, out var msg))
                {
                    await MessageHelper.EditAsync(channel, msg, string.Join(" ", args));
                    return;
                }
            }
        }
    }
}