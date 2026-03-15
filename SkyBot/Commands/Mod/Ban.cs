using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Commands
{
    public class Ban : ICommand
    {
        public string Name => "ban";
        public string[] Aliases => [];
        public string Description => "Bans a user from the planet.";
        public string Section => "mod";
        public string Usage => "ban <user> [reason]";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if (!PermissionHelper.HasPermAsync(member, [PlanetPermissions.Ban]).Result)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"You don't have permission to use this command.");
                    return;
                }

                string message = $"Work in progress...";
                await MessageHelper.ReplyAsync(ctx, channel, message);
            }
        }
    }
}