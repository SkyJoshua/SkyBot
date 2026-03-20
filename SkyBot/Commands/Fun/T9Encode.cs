using System.Collections.Concurrent;
using System.Text;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class T9Encode : ICommand
    {
        public string Name => "t9encode";
        public string[] Aliases => ["t9e"];
        public string Description => "Encodes text into old phone keypad multi-tap digits.";
        public string Section => "Fun";
        public string Usage => "t9encode <text> (or reply to a message)";

        // Maps each letter to (key digit, press count)
        private static readonly Dictionary<char, (char Key, int Presses)> Charmap = BuildCharmap();

        private static Dictionary<char, (char Key, int Presses)> BuildCharmap()
        {
            Dictionary<char, string> keymap = new()
            {
                ['2'] = "ABC",
                ['3'] = "DEF",
                ['4'] = "GHI",
                ['5'] = "JKL",
                ['6'] = "MNO",
                ['7'] = "PQRS",
                ['8'] = "TUV",
                ['9'] = "WXYZ",
            };

            var map = new Dictionary<char, (char, int)>();
            foreach (var (key, letters) in keymap)
                for (int i = 0; i < letters.Length; i++)
                    map[letters[i]] = (key, i + 1);

            return map;
        }

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            Message message = ctx.Message;
            string[] args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            string raw;

            if (message.ReplyToId.HasValue)
            {
                var replyMessage = await message.FetchReplyMessageAsync();
                raw = replyMessage?.Content ?? "";
            }
            else
            {
                if (args.Length == 0)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please provide text to encode, or reply to a message.");
                    return;
                }
                raw = string.Join(" ", args);
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "No text to encode.");
                return;
            }

            var result = new StringBuilder();
            char prevKey = '\0';

            foreach (char c in raw.ToUpper())
            {
                if (c == ' ')
                {
                    result.Append('0');
                    prevKey = '\0';
                    continue;
                }

                if (!Charmap.TryGetValue(c, out var entry))
                    continue;

                // If this letter uses the same key as the previous one, insert a 1 separator
                if (entry.Key == prevKey)
                    result.Append('1');

                result.Append(entry.Key, entry.Presses);
                prevKey = entry.Key;
            }

            string encoded = result.ToString().Trim('0');

            if (string.IsNullOrWhiteSpace(encoded))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Couldn't encode anything from that input.");
                return;
            }

            await MessageHelper.ReplyAsync(ctx, channel, $"📱 `{encoded}`");
        }
    }
}
