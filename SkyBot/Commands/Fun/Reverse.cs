using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Reverse : ICommand
    {
        public string Name => "reverse";
        public string[] Aliases => [];
        public string Description => "Reverses yours or a replied text.";
        public string Section => "Fun";
        public string Usage => "reverse [text] (Or reply to a message)";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;
            Message message = ctx.Message;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            string text;

            if (message.ReplyToId.HasValue)
            {
                var replyMessage = await message.FetchReplyMessageAsync();
                text = replyMessage?.Content ?? "";
            } else
            {
                if (args.Length == 0)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please provide some text to reverse or reply to a message.");
                    return;
                }
                text = string.Join(" ", args);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "No text to reverse.");
                return;
            }

            string reversed = new string(text.Reverse().ToArray());
            await MessageHelper.ReplyAsync(ctx, channel, $"Reversed: {reversed}");
        }
    }
}