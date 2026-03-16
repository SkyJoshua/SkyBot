using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class RockPaperScissors : ICommand
    {
        public string Name => "rps";
        public string[] Aliases => [];
        public string Description => "Play Rock Paper Scissors against the bot.";
        public string Section => "Fun";
        public string Usage => "rps <rock|paper|scissors>";

        private static readonly string[] Choices = ["rock", "paper", "scissors"];
        private static readonly string[] Emojis = ["🪨", "📄", "✂️"];

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;
            
            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            if (args.Length == 0)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please choose `Rock`, `Paper`, or `Scissors`.");
                return;
            }

            string input = args[0].ToLower();
            int playerIndex = Array.IndexOf(Choices, input);

            if (playerIndex == -1)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Invalid choice. Please choose `Rock`, `Paper`, or `Scissors`.");
                return;
            }

            TaskResult<Message> result = await MessageHelper.ReplyAsync(ctx, channel, "🤔 Thinking...");
            await Task.Delay(1000);

            int botIndex = Random.Shared.Next(3);

            string playerChoice = $"{Emojis[playerIndex]} {Choices[playerIndex].ToTitleCase()}";
            string botChoice = $"{Emojis[botIndex]} {Choices[botIndex].ToTitleCase()}";

            string outcome;
            if (playerIndex == botIndex) outcome = "It's a tie!";
            else if ((playerIndex == 0 && botIndex == 2) ||
                     (playerIndex == 1 && botIndex == 0) ||
                     (playerIndex == 2 && botIndex == 1)) outcome = "You win! 🎉";
            else outcome = "You Lose! 🥲";

            await MessageHelper.EditAsync(channel, result.Data, $"**You**: {playerChoice}\nvs\n**Bot**: {botChoice}\n[]()\n{outcome}");
        }
    }
}