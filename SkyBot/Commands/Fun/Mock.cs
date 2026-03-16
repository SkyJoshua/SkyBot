using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Mock : ICommand
    {
        public string Name => "mock";
        public string[] Aliases => [];
        public string Description => "Mock text";
        public string Section => "Fun";
        public string Usage => "mock [text] (Or reply to a message)";

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
                    await MessageHelper.ReplyAsync(ctx, channel, "Please provide some text to mock or reply to a message.");
                    return;
                }
                text = string.Join(" ", args);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "No text to mock.");
                return;
            }

            string mocked = new string(text.Select((c, i) => i % 2 == 0 ? char.ToLower(c) : char.ToUpper(c)).ToArray());
            await MessageHelper.ReplyAsync(ctx, channel, mocked);
        }
    }
}