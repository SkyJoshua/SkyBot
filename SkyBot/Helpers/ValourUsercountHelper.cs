using System.Text.Json;

namespace SkyBot.Helpers
{
    public static class ValourUsercountHelper {
        private static readonly HttpClient _http = new HttpClient();
        private static long _valourUsercount;
        public static long ValourUsercount => _valourUsercount;
        
        public static async Task UpdateUsercount()
        {
            try
            {
                var response = await _http.GetStringAsync("https://api.valour.gg/api/users/count");

                _valourUsercount = JsonSerializer.Deserialize<long>(response);

                Console.WriteLine($"Valour user count updated: {_valourUsercount}");
            } catch (Exception ex)
            {
                Console.WriteLine($"Failed to update valour user count: {ex.Message}");
            }
        }

        public static void StartUpdater()
        {
            var timer = new System.Timers.Timer(300_000);
            timer.Elapsed += async (_, _) => await UpdateUsercount();
            timer.AutoReset = true;
            timer.Start();
        }
    }
}