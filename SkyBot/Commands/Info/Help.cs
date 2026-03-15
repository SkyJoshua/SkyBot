using System.Collections.Concurrent;
using System.Text;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Commands
{
    public class Help : ICommand
    {
        public string Name => "help";
        public string[] Aliases => ["h"];
        public string Description => "Shows all the commands and their descriptions.";
        public string Section => "Info";
        public string Usage => "help|h [section] [page]";
        private const int PageSize = 5;

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;
            PlanetMember member = ctx.Member;

            bool isOwner = await PermissionHelper.IsOwner(member);

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            // Show all sections.
            if (args.Length == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("**Available Categories**");
                foreach (var section in CommandRegistry.Sections.Keys)
                {
                    if (section == "template") continue;
                    if (section == "dev" && !isOwner) continue;
                    if (section == "mod" && !PermissionHelper.HasPermAsync(member, [PlanetPermissions.Kick, PlanetPermissions.Ban, PlanetPermissions.ManageRoles]).Result) continue;
                    sb.AppendLine($"- `{section.ToTitleCase()}` ({CommandRegistry.Sections[section].Count})");
                }
                sb.AppendLine($"\nUse `{Config.Prefix}help <category>` to see commands in a category.");
                await MessageHelper.ReplyAsync(ctx, channel, sb.ToString());
                return;
            }

            // section [page]
            string sectionName = args[0].ToLower();
            if (!CommandRegistry.Sections.TryGetValue(sectionName, out var commands))
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"Unknown category `{sectionName}`.");
                return;
            }

            if (sectionName == "dev" && !isOwner)
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"Unknown category `{sectionName}`.");
                return;
            }

            if (sectionName == "mod" && !PermissionHelper.HasPermAsync(member, [PlanetPermissions.Kick, PlanetPermissions.Ban, PlanetPermissions.ManageRoles]).Result)
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"Unknown category `{sectionName}`.");
                return;
            }

            int page = 1;
            if (args.Length >= 2 && int.TryParse(args[1], out int parsedPage))
            {
                page = parsedPage;
            }

            int totalPages = (int)Math.Ceiling(commands.Count / (double)PageSize);
            page = Math.Clamp(page, 1, totalPages);
            
            var pageCommands = commands.Skip((page - 1) * PageSize).Take(PageSize);

            var sb2 = new StringBuilder();
            sb2.AppendLine($"**{sectionName.ToTitleCase()} commands** (Page {page}/{totalPages}):");
            foreach (var cmd in pageCommands)
            {
                var name = cmd.Aliases.Length > 0
                ? $"{cmd.Name}|{string.Join("|", cmd.Aliases)}"
                : cmd.Name;
                sb2.AppendLine($"`{Config.Prefix}{name}` - {cmd.Description}");
            }
            sb2.AppendLine($"\nUse `{Config.Prefix}help {sectionName} <page>` to see more.");

            await MessageHelper.ReplyAsync(ctx, channel, sb2.ToString());
        }
    }
}