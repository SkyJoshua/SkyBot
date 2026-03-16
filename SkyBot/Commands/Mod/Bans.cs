using System.Collections.Concurrent;
using System.Text;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class GetBans : ICommand
    {
        public string Name => "bans";
        public string[] Aliases => [""];
        public string Description => "Lists all bans in the planet.";
        public string Section => "Mod";
        public string Usage => "bans [page]";

        private const int PageSize = 10;

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            Planet planet = ctx.Planet;
            string[] args = ctx.Args;
            PlanetMember member = ctx.Member;

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if (!PermissionHelper.HasPerm(member, [PlanetPermissions.Ban]))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "You don't have permission to use this command.");
                    return;
                }

                int page = 1;
                if (args.Length > 0 && int.TryParse(args[0], out int parsedPage))
                    page = parsedPage;

                int skip = (page - 1) * PageSize;

                var queryResult = await planet.Node.PostAsyncWithResponse<QueryResponse<PlanetBan>>(
                    $"api/planets/{planet.Id}/bans/query",
                    new { skip, take = PageSize, options = new { } }
                );

                if (!queryResult.Success || queryResult.Data?.Items == null)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Failed to fetch bans.");
                    return;
                }

                if (!queryResult.Data.Items.Any())
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "No bans found.");
                    return;
                }

                int totalPages = (int)Math.Ceiling(queryResult.Data.TotalCount / (double)PageSize);

                var sb = new StringBuilder();
                sb.AppendLine($"**Bans** (Page {page}/{totalPages}):");

                IEnumerable<PlanetBan> activeBans = queryResult.Data.Items.Where(b => b.Permanent || b.TimeExpires > DateTime.UtcNow);

                if (!activeBans.Any())
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "No active bans found.");
                    return;
                }

                foreach (var ban in activeBans)
                {
                    var user = await ctx.Client.UserService.FetchUserAsync(ban.TargetId);
                    string username = user?.NameAndTag ?? "Unknown";
                    string expires = ban.TimeExpires.HasValue
                        ? $"{ban.TimeExpires}"
                        : "Never";
                    sb.AppendLine($"**{username}** `{user?.Id}` - {ban.Reason} (Expires: `{expires}`)");
                }

                sb.AppendLine($"\nUse `{Config.Prefix}bans <page>` to see more.");
                await MessageHelper.ReplyAsync(ctx, channel, sb.ToString());
            }
        }
    }
}