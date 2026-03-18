using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using SkyBot.Services;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class SetWelcome : ICommand
    {
        public string Name => "setwelcome";
        public string[] Aliases => [];
        public string Description => "Sets the welcome channel, message or active.";
        public string Section => "Mod";
        public string Usage => "set <channel|message|active [value]";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            Message message = ctx.Message;
            PlanetMember member = ctx.Member;
            Planet planet = ctx.Planet;
            string[] args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            if (!PermissionHelper.HasPerm(member, [PlanetPermissions.Manage]) && !PermissionHelper.IsOwner(member))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "You don't have permission to use this command.");
                return;
            }

            if (args.Length == 0)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please specify `channel` or `message`.");
                return;
            }

            switch (args[0].ToLower())
            {
                case "channel":
                case "c":
                    long targetChannelId;
                    if (message.Mentions != null && message.Mentions.Any(m => m.Type == MentionType.Channel)) {targetChannelId = message.Mentions.First(m => m.Type == MentionType.Channel).TargetId;}
                    else if (args.Length > 1 && long.TryParse(args[1], out long parsedChannelId)) {targetChannelId = parsedChannelId;}
                    else {targetChannelId = channelId;}

                    if (!channelCache.ContainsKey(targetChannelId)) {await MessageHelper.ReplyAsync(ctx, channel, "Could not find that channel."); return;}

                    await WelcomeService.SetWelcomeChannel(planet.Id, targetChannelId);
                    await MessageHelper.ReplyAsync(ctx, channel, $"Welcome channel set to «@c-{targetChannelId}».");
                    break;
                    
                case "message":
                case "m":
                    if (args.Length < 2)
                    {
                        await MessageHelper.ReplyAsync(ctx, channel, "Please provide a message. Valid variables: {username} {nickname} {fulluser} {mention} {id}");
                        return;
                    }
                    string msg = string.Join(" ", args[1..]);
                    await WelcomeService.SetWelcomeMessage( planet.Id, msg);
                    await MessageHelper.ReplyAsync(ctx, channel, $"Welcome message set to: `{msg}`");
                    break;

                case "active":
                case "a":
                    if (args.Length < 2)
                    {
                        await MessageHelper.ReplyAsync(ctx, channel, "Please provide a value. Use `true`, `false`, or `toggle`.");
                        return;
                    }
                    string value = args[1].ToLower();
                    if (value != "toggle" && value != "true" && value != "false")
                    {
                        await MessageHelper.ReplyAsync(ctx, channel, "Invalid value. Use `true`, `false`, `toggle`");
                        return;
                    }

                    if (value == "toggle")
                    {
                        var toggle = await WelcomeService.SetActive(planet.Id);
                        await MessageHelper.ReplyAsync(ctx, channel, toggle.Value ? "Welcome messages enabled." : "Welcome messages disabled.");
                        return;
                    }

                    bool.TryParse(value, out var active);

                    await WelcomeService.SetActive(planet.Id, active);
                    await MessageHelper.ReplyAsync(ctx, channel, active ? "Welcome messages enabled." : "Welcome messages disabled.");
                    break;

                default:
                    await MessageHelper.ReplyAsync(ctx, channel, "Invalid option. Use `channel`, `message` or `active`.");
                    break;
            }
            
        }
    }
}