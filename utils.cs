using System.Globalization;
using Valour.Sdk.Models;

namespace SkyBot
{
    public static class Utils
    {
        public static bool IsSingleEmoji(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            var enumerator = StringInfo.GetTextElementEnumerator(input);
            int count = 0;

            while (enumerator.MoveNext())
                count++;

            return count == 1;
        }

        public static bool ContainsAny(string input, params string[] values)
        {
            var lower = input.ToLower();

            foreach (var value in values)
            {
                if (lower.Contains(value.ToLower()))
                    return true;
            };

            return false;
        }

        public static async Task SendReplyAsync(Dictionary<long, Channel> channelCache, long channel, string reply)
        {
            if (channelCache.TryGetValue(channel, out var chan))
            {
                await chan.SendMessageAsync(reply);
            }
            else
            {
                Console.WriteLine($"Channel {channel} was not found in the cache.");
            };
        }
    };
};