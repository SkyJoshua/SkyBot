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
            if (!content.ToLower().StartsWith(prefix)) return;

            long channelId = message.ChannelId;
            PlanetMember member = await message.FetchAuthorMemberAsync();
            var parts = content.Substring(prefix.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            
            string command = parts[0].ToLower();
            string[] args = parts[1..];

            if (CommandRegistry.Commands.TryGetValue(command, out var handler))
            {
                await handler.Execute(new CommandContext
                {
                    ChannelCache = channelCache,
                    ChannelId = channelId,
                    Member = member,
                    Planet = message.Planet,
                    Args = args,
                    Message = message,
                    Client = client
                });
            } else
            {
                if (channelCache.TryGetValue(channelId, out var channel))
                {
                    await channel.SendMessageAsync($"{MentionHelper.Mention(member)} Unknown command.");
                }
            }
        }
    }
}