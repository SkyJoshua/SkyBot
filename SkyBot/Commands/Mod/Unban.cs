using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;
using Valour.Shared.Authorization;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class Unban : ICommand
    {
        public string Name => "unban";
        public string[] Aliases => [];
        public string Description => "Unbans a user from the planet.";
        public string Section => "Mod";
        public string Usage => "unban <id>";

        // public async Task Execute(CommandContext ctx)
        // {
        //     ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
        //     long channelId = ctx.ChannelId;

        //     if (channelCache.TryGetValue(channelId, out var channel))
        //     {
        //         await MessageHelper.ReplyAsync(ctx, channel, "Unbanning is currently unavailable due to a Valour server bug.");
        //     }
        // }

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
                    await MessageHelper.ReplyAsync(ctx, channel, "You don't have permission to use this command.");
                    return;
                }

                if (!PermissionHelper.HasPerm(bot, [PlanetPermissions.Ban]))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "I don't have permission to unban members.");
                    return;
                }

                if (args.Length < 1)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please provide a user ID.");
                    return;
                }

                if (!long.TryParse(args[0], out long targetUserId))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Invalid user ID.");
                    return;
                }

                PlanetBan? ban = null;
                int skip = 0;
                int take = 50;

                while (ban == null)
                {
                    var queryResult = await planet.Node.PostAsyncWithResponse<QueryResponse<PlanetBan>>(
                        $"api/planets/{planet.Id}/bans/query",
                        new {skip, take, options = new { }}
                    );

                    if (!queryResult.Success || queryResult.Data?.Items == null || !queryResult.Data.Items.Any())
                        break;
                    
                    ban = queryResult.Data.Items.FirstOrDefault(b => b.TargetId == targetUserId && (b.Permanent || b.TimeExpires > DateTime.UtcNow));

                    if (queryResult.Data.Items.Count < take)
                        break;

                    skip += take;
                }

                if (ban == null)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Could not find a ban for that user.");
                    return;
                }

                ban.TimeExpires = DateTime.UtcNow.AddSeconds(-1);
                TaskResult<PlanetBan> result = await planet.Node.PutAsyncWithResponse<PlanetBan>($"api/bans/{ban.Id}", ban);
                if (!result.Success)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Failed to unban.");
                    return;
                }

                User user = await ctx.Client.UserService.FetchUserAsync(targetUserId);
                await MessageHelper.ReplyAsync(ctx, channel, $"Unbanned `{user?.NameAndTag ?? targetUserId.ToString()}`.");
            }
        }
    }
}