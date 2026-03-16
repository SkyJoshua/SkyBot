using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Echo : ICommand
    {
        public string Name => "echo";
        public string[] Aliases => [];
        public string Description => "Echos what you said through the bot.";
        public string Section => "Fun";
        public string Usage => "echo <message>";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            string[] args = ctx.Args;
            Message message = ctx.Message;

            string reply = string.Join(" ", args);

            if (!channelCache.TryGetValue(channelId, out var channel)) return;
            if (string.IsNullOrWhiteSpace(reply)) await MessageHelper.ReplyAsync(ctx, channel, $"Enter a message to echo.");

            reply = $"{member.Name} » {reply}";
            if (reply.Length > 2048)
            {
                reply = reply.Substring(0, 2048);
            }

            await MessageHelper.ReplyAsync(ctx, channel, reply);
        }
    }
}