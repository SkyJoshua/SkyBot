using System.Collections.Concurrent;
using System.Text.Json;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class Cat : ICommand
    {
        public string Name => "cat";
        public string[] Aliases => [];
        public string Description => "Posts a random cat picture.";
        public string Section => "Chill";
        public string Usage => "cat";

        private static readonly HttpClient _http = new();

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            await channel.SendIsTyping();

            // Fetch a random cat from TheCatAPI
            string json;
            try
            {
                json = await _http.GetStringAsync("https://api.thecatapi.com/v1/images/search");
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "😿 Could not fetch a cat image. Try again later.");
                return;
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement[0];

            string catUrl = root.GetProperty("url").GetString()!;
            int width = root.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
            int height = root.TryGetProperty("height", out var h) ? h.GetInt32() : 0;

            string ext = Path.GetExtension(catUrl.Split('?')[0]).ToLowerInvariant();
            string mime = ext == ".png" ? "image/png"
                        : ext == ".gif" ? "image/gif"
                        : "image/jpeg";
            string fileName = $"cat{ext}";

            // Download the image bytes
            byte[] imageBytes;
            try
            {
                imageBytes = await _http.GetByteArrayAsync(catUrl);
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "😿 Could not download the cat image. Try again later.");
                return;
            }

            // Upload to Valour CDN so the server can scan/serve it
            string cdnUrl;
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
                form.Add(fileContent, "file", fileName);

                var uploadResult = await ctx.Planet.Node.PostMultipartDataWithResponse<string>("upload/image", form);
                if (!uploadResult.Success)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "😿 Could not upload the cat image. Try again later.");
                    return;
                }
                cdnUrl = uploadResult.Data!;
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "😿 Could not upload the cat image. Try again later.");
                return;
            }

            var attachment = new MessageAttachment(MessageAttachmentType.Image)
            {
                Location = cdnUrl,
                MimeType = mime,
                FileName = fileName,
                Width = width,
                Height = height
            };

            await channel.SendMessageAsync("",attachments: [attachment]);
        }
    }
}
