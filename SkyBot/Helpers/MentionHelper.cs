using Valour.Sdk.Models;

namespace SkyBot.Helpers
{
    public static class MentionHelper
    {
        public static string Mention(this PlanetMember member) => $"«@m-{member.Id}»";
        public static string Mention(this User user) => $"«@u-{user.Id}»";
    }
}