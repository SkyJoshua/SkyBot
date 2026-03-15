using Valour.Sdk.Client;
using Valour.Sdk.Models;
using SkyBot.Services;
using System.Collections.Concurrent;

namespace SkyBot
{
    public class SkyBot
    {
        private readonly ValourClient _client;
        private readonly ConcurrentDictionary<long, Channel> _channelCache = new();
        private readonly ConcurrentDictionary<long, bool> _initializedPlanets = new();

        public SkyBot()
        {
            _client = new ValourClient("https://api.valour.gg/");
            _client.SetupHttpClient();
        }

        public async Task StartAsync()
        {
            await BotService.InitializeBotAsync(_client, _channelCache, _initializedPlanets);
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            while (true)
            {
                try
                {
                    await new SkyBot().StartAsync();

                    Console.WriteLine("Ready and listening...");
                    await Task.Delay(Timeout.Infinite);
                } catch (InvalidOperationException ex) when (ex.Message.Contains("concurrent update"))
                {
                    Console.WriteLine("Concurrent update detected, restarting...");
                    await Task.Delay(1000);
                } catch (Exception ex)
                {
                    Console.WriteLine($"Fatal error: {ex.Message}");
                    break;
                }
            }
            
        }
    }
}
