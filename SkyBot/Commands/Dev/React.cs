using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class React : ICommand
    {
        public string Name => "react";
        public string[] Aliases => [];
        public string Description => "Send a message with a reaction at a set count.";
        public string Section => "Dev";
        public string Usage => "react <emoji> <amount> <message>";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            if (!PermissionHelper.IsOwner(ctx.Member))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "This is a Dev only command.");
                return;
            }

            string[] args = ctx.Args;

            if (args.Length < 3)
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"Usage: `{Config.Prefix}react <emoji> <amount> <message>`");
                return;
            }

            string emoji = args[0];

            if (!int.TryParse(args[1], out int amount) || amount < 1)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Amount must be a number between 1 and 100.");
                return;
            }

            string content = string.Join(" ", args.Skip(2));

            var reactions = Enumerable.Range(0, amount)
                .Select(_ => new MessageReaction
                {
                    Emoji         = emoji,
                    AuthorUserId  = ctx.Client.Me.Id,
                    AuthorMemberId = ctx.Planet.MyMember?.Id,
                    CreatedAt     = DateTime.UtcNow
                })
                .ToList();

            var msg = new Message(ctx.Client)
            {
                Content        = content,
                ChannelId      = channelId,
                PlanetId       = ctx.Planet.Id,
                AuthorUserId   = ctx.Client.Me.Id,
                AuthorMemberId = ctx.Planet.MyMember?.Id,
                Reactions      = reactions,
                Fingerprint    = Guid.NewGuid().ToString()
            };

            await ctx.Client.MessageService.SendMessage(msg);
        }
    }
}
