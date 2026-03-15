using System.Collections.Concurrent;
using SkyBot.Helpers;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public static class HelpCommand
    {
        public static async Task Execute(ConcurrentDictionary<long, Channel> channelCache, long channelId, String prefix, PlanetMember member)
        {
            string helpMessage = $@"**Skybot Commands**:
            - `s/echo <text>` - Echos text into the chat
            - `s/suggest` - Shares the suggestions link
            - `s/source` - Sends link for the source code
            - `s/joincode` - Sends a link to a github that you can use to make your bot join your planet.
            - `s/joinsite` - Sends a link to a website that you can use to make yout bot join your planet.
            - `s/api|swagger` - Sends a link to the Swagger API
            - `s/cmds|help` - Shows this list
            - `s/usercount` - Shows the user count of Valour
            - `s/devcentral` - Sends the invite link to the Dev Central Planet
            - `s/mc` - Sends Unofficial ValourSMP IPs
            ";

            if (channelCache.TryGetValue(channelId, out var channel))
            {
                await channel.SendMessageAsync($"{MentionHelper.Mention(member)}\n{helpMessage}");
            }
        }
    }
}