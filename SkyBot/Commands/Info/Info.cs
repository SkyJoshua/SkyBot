using System.Collections.Concurrent;
using System.Text;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Info : ICommand
    {
        public string Name => "info";
        public string[] Aliases => [];
        public string Description => "Shows the info about a User or the Planet.";
        public string Section => "Info";
        public string Usage => "info <user|planet> [mention|memberid]";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            if (args.Length == 0)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please specify `user` or `planet`.");
                return;
            }

            switch (args[0].ToLower())
            {
                case "user":
                case "u":
                    await HandleUserInfo(ctx, channel);
                    break;
                case "planet":
                case "p":
                    await HandlePlanetInfo(ctx, channel);
                    break;
                default:
                    await MessageHelper.ReplyAsync(ctx, channel, "invalid option. Use `user` or `planet`.");
                    break;
            }
        }

        private async Task HandleUserInfo(CommandContext ctx, Channel channel)
        {
            Message message = ctx.Message;
            Planet planet = ctx.Planet;
            string[] args = ctx.Args;
            PlanetMember? target;

            try
            {
                if (message.Mentions != null && message.Mentions.Any())
                {
                    target = await planet.FetchMemberAsync(message.Mentions.First().TargetId);
                }
                else if (args.Length > 1 && long.TryParse(args[1], out long memberid))
                {
                    target = await planet.FetchMemberAsync(memberid);
                }
                else
                {
                    target = ctx.Member;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                target = ctx.Member;
            }

            if (target == null)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Could not find that member.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"**{target.Name.Trim()}**");
            sb.AppendLine($"User ID: `{target.UserId}`");
            sb.AppendLine($"Member ID: `{target.Id}`");
            sb.AppendLine($"Nickname: `{(string.IsNullOrWhiteSpace(target.Nickname) ? "None" : target.Nickname)}`");
            sb.AppendLine($"Subscription: `{(string.IsNullOrWhiteSpace(target.User.SubscriptionType) ? "None" : target.User.SubscriptionType)}`");
            sb.AppendLine($"Status: `{(string.IsNullOrWhiteSpace(target.Status) ? "None" : target.Status)}`");
            sb.AppendLine($"Primary Role: `{target.PrimaryRole?.Name ?? "None"}`");
            sb.AppendLine($"Roles: `{string.Join(", ", target.Roles.Select(r => r.Name))}`");

            await MessageHelper.ReplyAsync(ctx, channel, sb.ToString());
        }

        private async Task HandlePlanetInfo(CommandContext ctx, Channel channel)
        {
            var planet = ctx.Planet;

            var sb = new StringBuilder();
            sb.AppendLine($"**{planet.Name}**");
            sb.AppendLine($"Planet ID: `{planet.Id}`");
            sb.AppendLine($"Owner ID: `{planet.OwnerId}`");
            sb.AppendLine($"Member Count: `{planet.Members?.Count ?? 0}`");
            sb.AppendLine($"Channel Count: `{planet.Channels?.Count ?? 0}`");
            sb.AppendLine($"Role Count: `{planet.Roles?.Count ?? 0}`");
            sb.AppendLine($"Description: `{(string.IsNullOrWhiteSpace(planet.Description) ? "None" : planet.Description)}`");

            await MessageHelper.ReplyAsync(ctx, channel, sb.ToString());
        }
    }
}