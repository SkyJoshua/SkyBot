using System.Collections.Concurrent;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Commands
{
    public class Choose : ICommand
    {
        public string Name => "choose";
        public string[] Aliases => ["pick"];
        public string Description => "Picks one of the given options.";
        public string Section => "Fun";
        public string Usage => "choose <option1> <option2> ...";

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;
            
            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            if (ctx.Args.Length < 2)
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please provide at least two options.");
                return;
            }

            TaskResult<Message> result = await MessageHelper.ReplyAsync(ctx, channel, "🤔 Choosing...");
            await Task.Delay(1000);

            string choice = args[Random.Shared.Next(args.Length)];
            await MessageHelper.EditAsync(channel, result.Data, $"I choose **{choice}**!");
        }
    }
}