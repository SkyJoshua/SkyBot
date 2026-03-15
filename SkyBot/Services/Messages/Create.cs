using System.Collections.Concurrent;
using Skybot;
using SkyBot.Commands;
using SkyBot.Helpers;
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
            string prefix = Config.Prefix;

            if (message.AuthorUserId == client.Me.Id) return;

            string content = message.Content ?? "";
            if (string.IsNullOrWhiteSpace(content)) return;
            if (!content.StartsWith(prefix)) return;

            long channelId = message.ChannelId;

            PlanetMember member = await message.FetchAuthorMemberAsync();
            
            var parts = content.Substring(prefix.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            
            string command = parts[0].ToLower();
            string[] args = parts[1..];

            switch (command)
            {
                case "help":
                    await HelpCommand.Execute(channelCache, channelId, prefix, member);
                    break;

                default:
                    if (channelCache.TryGetValue(channelId, out var channel))
                    {
                        await channel.SendMessageAsync($"{MentionHelper.Mention(member)} Unknown command.");
                    }
                    break;
            }
        }
    }
}