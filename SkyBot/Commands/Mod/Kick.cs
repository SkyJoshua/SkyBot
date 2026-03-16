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
        public string Section => "Mod";
        public string Usage => "kick <user> [reason]";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            Planet planet = ctx.Planet;
            Message message = ctx.Message;
            string[] args = ctx.Args;
            PlanetMember member = ctx.Member;
            PlanetMember bot = await planet.FetchMemberByUserAsync(ctx.Client.Me.Id);

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if (!PermissionHelper.HasPerm(member, [PlanetPermissions.Kick]))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"You don't have permission to use this command.");
                    return;
                }

                if (!PermissionHelper.HasPerm(bot, [PlanetPermissions.Kick]))   
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"I don't have permission to kick members.");
                    return;
                }

                if (!message.Mentions.Any() && args.Length < 2)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please mention someone or user their id.");
                    return;
                }

                
                long targetId;
                if (message.Mentions.Any())
                {
                    targetId = message.Mentions.First().TargetId;
                }
                else if (!long.TryParse(args[1], out targetId))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Invalid member ID.");
                    return;
                }
                PlanetMember victim = await planet.FetchMemberAsync(targetId);

                if (victim == null)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Could not find that member.");
                    return;
                }

                string reason = args.Length > 1 && !message.Mentions.Any()
                    ? string.Join(" ", args[2..])
                    : args.Length > 1
                        ? string.Join(" ", args[1..])
                        : "No reason provided.";

                await victim.DeleteAsync();
                await MessageHelper.ReplyAsync(ctx, channel, $"Kicked {victim.Name}. Reason: {reason}");
            }
        }
    }
}