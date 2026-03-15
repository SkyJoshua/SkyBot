using SkyBot.Models;
using Valour.Sdk.Models;

public static class MessageHelper
{
    public static async Task ReplyAsync(CommandContext ctx, Channel channel, string content)
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
        await ctx.Client.MessageService.SendMessage(msg);
    }

    public static string ToTitleCase(this string str) => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
}