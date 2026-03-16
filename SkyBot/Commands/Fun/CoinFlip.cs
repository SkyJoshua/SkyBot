using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class CoinFlip : ICommand
    {
        public string Name => "coinflip";
        public string[] Aliases => ["cf"];
        public string Description => "Flips a coin.";
        public string Section => "Fun";
        public string Usage => "coinflip";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            
            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            TaskResult<Message> result = await MessageHelper.ReplyAsync(ctx, channel, "🪙 Flipping...");
            await channel.SendIsTyping();
            await Task.Delay(3000);

            string outcome = Random.Shared.Next(2) == 0 ? "Heads" : "Tails";
            await MessageHelper.EditAsync(channel, result.Data, $"🪙 {outcome}");
        }
    }
}