using System.Globalization;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Valour.Sdk.Models;

namespace SkyBot
{
    public static class Utils
    {

        private static readonly HttpClient _http = new HttpClient();
        private static long _valourUserCount;

        public static long ValourUserCount => _valourUserCount;



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

        public static async Task UpdateValourUserCountAsync()
        {
            try
            {
                var response = await _http.GetStringAsync("https://api.valour.gg/api/users/count");

                _valourUserCount = JsonSerializer.Deserialize<long>(response);

                Console.WriteLine($"Valour user count updated: {_valourUserCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update Valour user count: {ex.Message}");
            }
        }

        public static void StartValourUserUpdater()
        {
            var timer = new System.Timers.Timer(300_000);
            timer.Elapsed += async (_, _) => await UpdateValourUserCountAsync();
            timer.AutoReset = true;
            timer.Start();
        }
    };
};