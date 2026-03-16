using SkyBot.Models;
using Valour.Sdk.Models;
using Valour.Shared;

namespace SkyBot.Helpers
{
    public static class MessageHelper
    {

        public static string Mention(this PlanetMember member) => $"«@m-{member.Id}»";
        public static string Mention(this User user) => $"«@u-{user.Id}»";
        public static string ToTitleCase(this string str) => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        public static async Task<TaskResult<Message>> ReplyAsync(CommandContext ctx, Channel channel, string content)
        {
            long? replyToId = ctx.Message.ReplyToId.HasValue ? ctx.Message.ReplyToId : ctx.Message.Id;

            var msg = new Message(ctx.Client)
            {
                Content = content,
                ChannelId = channel.Id,
                PlanetId = ctx.Planet.Id,
                AuthorUserId = ctx.Client.Me.Id,
                AuthorMemberId = channel.Planet?.MyMember.Id,
                ReplyToId = replyToId,
                Fingerprint = Guid.NewGuid().ToString()
            };
            return await ctx.Client.MessageService.SendMessage(msg);
        }
        public static async Task<TaskResult<Message>> EditAsync(Channel channel, Message message, string content)
        {
            message.Content = content;
            return await channel.Planet.Node.PutAsyncWithResponse<Message>($"api/messages/{message.Id}", message);
        }
        public static DateTime? ParseDuration(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var unit = input[^1];
            if (!int.TryParse(input[..^1], out int value)) return null;

            return unit switch
            {
                'm' => DateTime.UtcNow.AddMinutes(value),
                'h' => DateTime.UtcNow.AddHours(value),
                'd' => DateTime.UtcNow.AddDays(value),
                'w' => DateTime.UtcNow.AddDays(value * 7),
                'M' => DateTime.UtcNow.AddMonths(value),
                'y' => DateTime.UtcNow.AddYears(value),
                _ => null
            };
        }
    }
}