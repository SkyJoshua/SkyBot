using System.Collections.Concurrent;
using System.Text;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class MultiTap : ICommand
    {
        public string Name => "t9decode";
        public string[] Aliases => ["t9d"];
        public string Description => "Decodes old phone keypad multi-tap input into text.";
        public string Section => "Fun";
        public string Usage => "multitap <digits> (e.g. 44 3 555 555 666 or reply to a message)";

        private static readonly Dictionary<char, string> Keymap = new()
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
                    await MessageHelper.ReplyAsync(ctx, channel, "Please provide digits to decode, or reply to a message. Example: `multitap 44 3 555 555 666`");
                    return;
                }
                raw = string.Join("", args);
            }

            // Strip anything that isn't a digit
            string input = new([..raw.Where(char.IsDigit)]);

            var result = new StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                char digit = input[i];

                if (digit == '0')
                {
                    result.Append(' ');
                    i++;
                    continue;
                }

                // 1 = silent same-key separator if surrounded by the same digit, otherwise a space
                if (digit == '1')
                {
                    int j = i;
                    while (j < input.Length && input[j] == '1') j++;

                    char before = i > 0 ? input[i - 1] : '\0';
                    char after  = j < input.Length ? input[j] : '\0';
                    bool sameKey = before >= '2' && before <= '9' && before == after;

                    if (!sameKey) result.Append(' ');
                    i = j;
                    continue;
                }

                if (!Keymap.TryGetValue(digit, out string? letters))
                {
                    i++;
                    continue;
                }

                // Count consecutive presses of the same digit
                int count = 0;
                while (i + count < input.Length && input[i + count] == digit)
                    count++;

                int letterIndex = (count - 1) % letters.Length;
                result.Append(letters[letterIndex]);
                i += count;
            }

            string decoded = result.ToString().Trim();

            if (string.IsNullOrWhiteSpace(decoded))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Couldn't decode anything from that input.");
                return;
            }

            await MessageHelper.ReplyAsync(ctx, channel, $"📱 **{decoded}**");
        }
    }
}
