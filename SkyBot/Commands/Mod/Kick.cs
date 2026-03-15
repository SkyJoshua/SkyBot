using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Commands
{
    public class Kick : ICommand
    {
        public string Name => "kick";
        public string[] Aliases => [];
        public string Description => "Kicks a user from the planet.";
        public string Section => "mod";
        public string Usage => "kick <user> [reason]";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if (!PermissionHelper.HasPermAsync(member, [PlanetPermissions.Kick]).Result)
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