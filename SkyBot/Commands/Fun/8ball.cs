using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class EightBall : ICommand
    {
        public string Name => "8ball";
        public string[] Aliases => [];
        public string Description => "Ask the magic 8ball a question.";
        public string Section => "Fun";
        public string Usage => "8ball <question>";

        private static readonly string[] Responses =
        [
            "It is certain.",
            "It is decidedly so.",
            "Without a doubt.",
            "Yes, definitely.",
            "You may rely on it.",
            "As I see it, yes.",
            "Most likely.",
            "Outlook good.",
            "Yes.",
            "Signs point to yes.",
            "Reply hazy, try again.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            "Don't count on it.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Very doubtful."
        ];

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                if (args.Length == 0)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Please ask a question.");
                    return;
                }

                
                TaskResult<Message> result = await MessageHelper.ReplyAsync(ctx, channel, $"🎱 Thinking...");
                await channel.SendIsTyping();
                await Task.Delay(2000);
                string response = Responses[Random.Shared.Next(Responses.Length)];
                await MessageHelper.EditAsync(channel, result.Data, $"🎱 {response}");
            }
        }
    }
}