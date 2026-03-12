using System.Globalization;
using System.Text.Json;
using Valour.Sdk.Models;
using Valour.Sdk.Client;
using System.Text;

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

        public static async Task InitializePlanetsAsync(ValourClient client, Dictionary<long, Channel> channelCache, HashSet<long> initializedPlanets)
        {
            foreach (var planet in client.PlanetService.JoinedPlanets)
            {
                if (initializedPlanets.Contains(planet.Id))
                    continue;

                Console.WriteLine($"Initializing Planet: {planet.Name}");

                await planet.EnsureReadyAsync();
                await planet.FetchInitialDataAsync();

                foreach (var channel in planet.Channels)
                {
                    channelCache[channel.Id] = channel;

                    if (channel.ChannelType == Valour.Shared.Models.ChannelTypeEnum.PlanetChat)
                    {
                        await channel.OpenWithResult("SkyBot");
                        Console.WriteLine($"Realtime opened for: {planet.Name} -> {channel.Name}");
                    }
                }

                initializedPlanets.Add(planet.Id);
            }
        }

        public class ReactionInterceptor : TextWriter
        {
            private readonly TextWriter _original;
            public bool DetectedAlreadyExists { get; private set; }
            public override Encoding Encoding => _original.Encoding;

            public ReactionInterceptor(TextWriter original)
            {
                _original = original;
            }

            public void Reset() => DetectedAlreadyExists = false;

            public override void WriteLine(string value)
            {
                if (value?.Contains("Reaction already exists") == true)
                    DetectedAlreadyExists = true;
                _original.WriteLine(value);
            }

            public override void Write(string value)
            {
                if (value?.Contains("Reaction already exists") == true)
                    DetectedAlreadyExists = true;
                _original.Write(value);
            }
        }
    };
};