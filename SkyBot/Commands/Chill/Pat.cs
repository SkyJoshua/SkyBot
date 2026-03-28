using System.Collections.Concurrent;
using System.Text.Json;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace SkyBot.Commands
{
    public class Pat : ICommand
    {
        public string Name => "pat";
        public string[] Aliases => [];
        public string Description => "Give someone headpats with a random gif.";
        public string Section => "Chill";
        public string Usage => "pat [@user]";

        private static readonly HttpClient _http = new()
        {
            DefaultRequestHeaders = { { "User-Agent", "SkyBot/1.0" } }
        };

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            await channel.SendIsTyping();

            // Fetch a random pat gif from nekos.best
            string json;
            try
            {
                json = await _http.GetStringAsync("https://nekos.best/api/v2/pat");
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Could not fetch a pat gif. Try again later.");
                return;
            }

            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement.GetProperty("results");
            string gifUrl = results[0].GetProperty("url").GetString()!;

            // Download the gif bytes
            byte[] gifBytes;
            try
            {
                gifBytes = await _http.GetByteArrayAsync(gifUrl);
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Could not download the pat gif. Try again later.");
                return;
            }

            // Read GIF dimensions from header (bytes 6-9, little-endian)
            int width = 0, height = 0;
            if (gifBytes.Length >= 10)
            {
                width  = gifBytes[6] | (gifBytes[7] << 8);
                height = gifBytes[8] | (gifBytes[9] << 8);
            }

            // Upload to Valour CDN
            string cdnUrl;
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(gifBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/gif");
                form.Add(fileContent, "file", "pat.gif");

                var uploadResult = await ctx.Planet.Node.PostMultipartDataWithResponse<string>("upload/image", form);
                if (!uploadResult.Success)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Could not upload the pat gif. Try again later.");
                    return;
                }
                cdnUrl = uploadResult.Data!;
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Could not upload the pat gif. Try again later.");
                return;
            }

            // Resolve the pat target: replied-to message author > args > nobody
            string? target = null;
            if (ctx.Message.ReplyToId is not null)
            {
                var replied = await ctx.Message.FetchReplyMessageAsync();
                if (replied is not null)
                {
                    var author = await replied.FetchAuthorAsync();
                    if (author is not null)
                        target = author.Name;
                }
            }
            if (target is null && ctx.Args.Length > 0)
                target = string.Join(" ", ctx.Args);

            string sender = ctx.Member.Nickname ?? ctx.Member.User?.Name ?? "Unknown";
            string text = target is not null
                ? $"{sender} gives {target} headpats! 🥰"
                : $"{sender} wants headpats! 🥰";

            var attachment = new MessageAttachment(MessageAttachmentType.Image)
            {
                Location = cdnUrl,
                MimeType = "image/gif",
                FileName = "pat.gif",
                Width = width,
                Height = height
            };

            await channel.SendMessageAsync(text, attachments: [attachment]);
        }
    }
}
