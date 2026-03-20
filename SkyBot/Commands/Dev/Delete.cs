using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Delete : ICommand
    {
        public string Name => "delete";
        public string[] Aliases => ["del"];
        public string Description => "Delete a bot message";
        public string Section => "Dev";
        public string Usage => "reply -> delete";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            ValourClient client = ctx.Client;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            Message message = ctx.Message;

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if (!PermissionHelper.IsOwner(member))
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
                    await msg.DeleteAsync();
                }
            }
        }
    }
}
