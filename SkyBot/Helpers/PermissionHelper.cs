using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Helpers
{
    public static class PermissionHelper
    {
        // Planet-level permissions
        public static bool HasPerm(PlanetMember member, PlanetPermission[] permissions, bool requireAll = false)
        {
            if (member == null) return false;
            if (member.HasPermission(PlanetPermissions.FullControl)) return true;
            if (member.Roles.Any(r => r.IsAdmin)) return true;

            return requireAll
                ? permissions.All(p => member.HasPermission(p))
                : permissions.Any(p => member.HasPermission(p));
        }

        // Chat channel permissions
        public static async Task<bool> HasPermAsync(PlanetMember member, Channel channel, ChatChannelPermission[] permissions, bool requireAll = false)
        {
            if (member == null) return false;
            if (member.HasPermission(PlanetPermissions.FullControl)) return true;
            if (member.Roles.Any(r => r.IsAdmin)) return true;

            if (requireAll)
            {
                foreach (var p in permissions)
                    if (!await channel.HasPermissionAsync(member, p)) return false;
                return true;
            }
            else
            {
                foreach (var p in permissions)
                    if (await channel.HasPermissionAsync(member, p)) return true;
                return false;
            }
        }

        // Voice channel permissions
        public static async Task<bool> HasPermAsync(PlanetMember member, Channel channel, VoiceChannelPermission[] permissions, bool requireAll = false)
        {
            if (member == null) return false;
            if (member.HasPermission(PlanetPermissions.FullControl)) return true;
            if (member.Roles.Any(r => r.IsAdmin)) return true;

            if (requireAll)
            {
                foreach (var p in permissions)
                    if (!await channel.HasPermissionAsync(member, p)) return false;
                return true;
            }
            else
            {
                foreach (var p in permissions)
                    if (await channel.HasPermissionAsync(member, p)) return true;
                return false;
            }
        }

        public static bool IsOwner(PlanetMember member)
        {
            if (member == null) return false;
            return member.UserId == Config.OwnerId;
        }
    }
}