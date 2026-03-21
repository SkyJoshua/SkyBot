using System.Collections.Concurrent;
using System.Text.Json;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class Image : ICommand
    {
        public string Name => "image";
        public string[] Aliases => ["img"];
        public string Description => "Fetches a random image matching your search.";
        public string Section => "Fun";
        public string Usage => "image <query>";

        private static readonly HttpClient _http = new();
        private static readonly Random _rng = new();

        private record ImageResult(string Url, int Width, int Height, string Mime);

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            string[] args = ctx.Args;
            if (args.Length == 0)
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"Usage: `{Config.Prefix}image <query>`");
                return;
            }

            string query = string.Join(" ", args);
            await channel.SendIsTyping();

            var result = await FetchPixabayAsync(query) ?? await FetchWikimediaAsync(query);

            if (result is null)
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"🔍 No images found for **{query}**.");
                return;
            }

            string ext      = Path.GetExtension(result.Url.Split('?')[0]).ToLowerInvariant();
            string fileName = $"image{(string.IsNullOrEmpty(ext) ? ".jpg" : ext)}";

            byte[] imageBytes;
            try
            {
                imageBytes = await _http.GetByteArrayAsync(result.Url);
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "🖼️ Could not download the image. Try again later.");
                return;
            }

            string cdnUrl;
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(result.Mime);
                form.Add(fileContent, "file", fileName);

                var uploadResult = await ctx.Planet.Node.PostMultipartDataWithResponse<string>("upload/image", form);
                if (!uploadResult.Success)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "🖼️ Could not upload the image. Try again later.");
                    return;
                }
                cdnUrl = uploadResult.Data;
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "🖼️ Could not upload the image. Try again later.");
                return;
            }

            var attachment = new MessageAttachment(MessageAttachmentType.Image)
            {
                Location = cdnUrl,
                MimeType = result.Mime,
                FileName = fileName,
                Width    = result.Width,
                Height   = result.Height
            };

            await channel.SendMessageAsync($"🖼️ **{query}**", attachments: [attachment]);
        }

        private static async Task<ImageResult?> FetchPixabayAsync(string query)
        {
            string? key = Environment.GetEnvironmentVariable("PIXABAY_API_KEY");
            if (string.IsNullOrWhiteSpace(key)) return null;

            string url = $"https://pixabay.com/api/?key={key}" +
                         $"&q={Uri.EscapeDataString(query)}&image_type=photo&per_page=20&safesearch=true";

            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync(url);
            }
            catch { return null; }

            // 429 = rate limited — fall through to Wikimedia
            if (!response.IsSuccessStatusCode) return null;

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var hits = doc.RootElement.GetProperty("hits");
            if (hits.GetArrayLength() == 0) return null;

            var hit    = hits[_rng.Next(hits.GetArrayLength())];
            string imgUrl = hit.GetProperty("webformatURL").GetString()!;
            int width  = hit.TryGetProperty("webformatWidth",  out var w) ? w.GetInt32() : 0;
            int height = hit.TryGetProperty("webformatHeight", out var h) ? h.GetInt32() : 0;
            string ext = Path.GetExtension(imgUrl.Split('?')[0]).ToLowerInvariant();
            string mime = ext == ".png" ? "image/png" : ext == ".gif" ? "image/gif" : "image/jpeg";

            return new ImageResult(imgUrl, width, height, mime);
        }

        private static async Task<ImageResult?> FetchWikimediaAsync(string query)
        {
            string url = "https://commons.wikimedia.org/w/api.php" +
                         $"?action=query&generator=search&gsrsearch=intitle:{Uri.EscapeDataString(query)}" +
                         "&gsrnamespace=6&gsrlimit=30&prop=imageinfo&iiprop=url|size|mime&format=json";

            string json;
            try
            {
                _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SkyBot/1.0 (https://github.com/SkyJoshua/SkyBot)");
                json = await _http.GetStringAsync(url);
            }
            catch { return null; }

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("query", out var queryEl) ||
                !queryEl.TryGetProperty("pages", out var pages)) return null;

            var candidates = new List<ImageResult>();
            foreach (var page in pages.EnumerateObject())
            {
                if (!page.Value.TryGetProperty("imageinfo", out var infoArr)) continue;
                var info = infoArr[0];

                string mime = info.TryGetProperty("mime", out var m) ? m.GetString() ?? "" : "";
                if (mime is not ("image/jpeg" or "image/png" or "image/gif" or "image/webp")) continue;

                string imgUrl = info.TryGetProperty("url",    out var u) ? u.GetString() ?? "" : "";
                int width     = info.TryGetProperty("width",  out var w) ? w.GetInt32()  : 0;
                int height    = info.TryGetProperty("height", out var h) ? h.GetInt32()  : 0;

                if (!string.IsNullOrEmpty(imgUrl))
                    candidates.Add(new ImageResult(imgUrl, width, height, mime));
            }

            return candidates.Count == 0 ? null : candidates[_rng.Next(candidates.Count)];
        }
    }
}
