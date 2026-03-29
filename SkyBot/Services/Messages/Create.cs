using System.Collections.Concurrent;
using SkyBot.Commands;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;



namespace SkyBot.Services.Messages
{
    public static class Create
    {
        private static readonly ConcurrentDictionary<long, DateTime> _cooldowns = new();
        private static readonly TimeSpan _cooldown = TimeSpan.FromSeconds(2);
        public static async Task MessageAsync(
            ValourClient client,
            ConcurrentDictionary<long, Channel> channelCache,
            Message message
        )
        {
            if (message.AuthorUserId == client.Me.Id) return;
            string prefix = Config.Prefix;
            string content = message.Content ?? "";
            if (string.IsNullOrWhiteSpace(content)) return;
            long channelId = message.ChannelId;
            PlanetMember member = await message.FetchAuthorMemberAsync();
            noPrefixMessages(message, content);
            var parts = content.Substring(prefix.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            string command = parts[0].ToLower();
            string[] args = parts[1..];
            CommandContext ctx = new CommandContext
            {
                ChannelCache = channelCache,
                ChannelId = channelId,
                Member = member,
                Planet = message.Planet,
                Args = args,
                Message = message,
                Client = client
            };

            async void noPrefixMessages(Message message, string content)
            {
                if (message.AuthorUserId == Config.OwnerId)
                {
                    if (MessageHelper.IsSingleEmoji(content))
                    {
                        await message.AddReactionAsync(content);
                    }
                }

                // await message.AddReactionAsync("🫃");
            }


            if (!content.ToLower().StartsWith(prefix)) return;
            if (_cooldowns.TryGetValue(message.AuthorUserId, out var lastUsed) && DateTime.UtcNow - lastUsed < _cooldown)
                return;

            _cooldowns[message.AuthorUserId] = DateTime.UtcNow;

            if (CommandRegistry.Commands.TryGetValue(command, out var handler))
            {
                await handler.Execute(ctx);
            } else
            {
                if (channelCache.TryGetValue(channelId, out var channel))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, $"Unknown command `{command}`.");
                }
            }
        }
    }
}