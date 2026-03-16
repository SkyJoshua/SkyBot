using System.Collections.Concurrent;
using System.Runtime.InteropServices.Marshalling;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class Dice : ICommand
    {
        public string Name => "dice";
        public string[] Aliases => ["roll"];
        public string Description => "Rolls dice.";
        public string Section => "Fun";
        public string Usage => "roll <dice> (e.g. 2d6, d20, 3d8)";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            string[] args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            string input = args.Length > 0 ? args[0].ToLower() : "1d6";

            string[] parts = input.Split('d');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int sides) || sides < 2)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Invalid dice format. Use something like `2d6` or `d20`.");
                return;
            }

            int count = 1;
            if (!string.IsNullOrWhiteSpace(parts[0]) && !int.TryParse(parts[0], out count))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Invalid dice format. Use something like `2d6` or `d20`.");
                return;
            }

            if (count < 1 || count > 100)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "You can only roll between 1 and 100 dice at a time.");
                return;
            }

            IEnumerable<int> rolls = Enumerable.Range(0, count).Select(_ => Random.Shared.Next(1, sides+1)).ToList();
            int total = rolls.Sum();

            TaskResult<Message> rolling = await MessageHelper.ReplyAsync(ctx, channel, "🎲 Rolling...");
            await channel.SendIsTyping();
            await Task.Delay(2000);

            string rollDisplay = count > 1 ? $"({string.Join(" + ", rolls)}) = **{total}**" : $"**{total}**";
            await MessageHelper.EditAsync(channel, rolling.Data, $"🎲 Rolled {input}: {rollDisplay}");
        }
    }
}