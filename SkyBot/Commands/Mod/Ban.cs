using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;
using Valour.Shared.Authorization;

namespace SkyBot.Commands
{
    public class Ban : ICommand
    {
        public string Name => "ban";
        public string[] Aliases => [];
        public string Description => "Bans a user from the planet.";
        public string Section => "Mod";
        public string Usage => "ban <mention|memberid> [reason] [length (y=year, M=month, w=week, d=day, h=hour, m=minute)]";

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
                if (!PermissionHelper.HasPerm(member, [PlanetPermissions.Ban]))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"You don't have permission to use this command.");
                    return;
                }

                if (!PermissionHelper.HasPerm(bot, [PlanetPermissions.Ban]))   
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"I don't have permission to ban members.");
                    return;
                }

                if (!message.Mentions.Any() && args.Length < 1)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please mention someone or user their id.");
                    return;
                }

                
                long targetId;
                if (message.Mentions.Any())
                {
                    targetId = message.Mentions.First().TargetId;
                }
                else if (!long.TryParse(args[0], out targetId))
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

                DateTime? expires = null;
                List<string> remainingArgs = args[1..].ToList();

                for (int i = 0; i < remainingArgs.Count; i++)
                {
                    var parsed = MessageHelper.ParseDuration(remainingArgs[i]);
                    if (parsed != null)
                    {
                        expires = parsed;
                        remainingArgs.RemoveAt(i);
                        break;
                    }
                }

                string reason = remainingArgs.Count > 0 ? string.Join(" ", remainingArgs) : "No reason provided.";

                PlanetBan ban = new PlanetBan(ctx.Client)
                {
                    PlanetId = planet.Id,
                    TargetId = victim.UserId,
                    IssuerId = ctx.Client.Me.Id,
                    Reason = reason,
                    TimeCreated = DateTime.UtcNow,
                    TimeExpires = expires
                };

                

                TaskResult<PlanetBan> result = await ban.CreateAsync();
                if (!result.Success)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"Failed to ban {victim.Name}: {result.Message}");
                    return;
                }

                if (expires == null)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"Permanently Banned `{victim.Name}`. Reason: `{reason}`");
                } else
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"Banned `{victim.Name}` until `{expires}`. Reason: `{reason}`");
                }

                
            }
        }

        
    }
}