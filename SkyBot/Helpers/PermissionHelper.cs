using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Helpers
{
    public static class PermissionHelper
    {
        public static async Task<bool> HasPermAsync(PlanetMember member, PlanetPermission[] permissions, bool requireAll = false)
        {
            if (member == null) return false;
            if (member.HasPermission(PlanetPermissions.FullControl)) return true;
            if (member.Roles.Any(r => r.IsAdmin)) return true;
            
            return requireAll
                ? permissions.All(permission => member.HasPermission(permission))
                : permissions.Any(permission => member.HasPermission(permission));
        }

        public static async Task<bool> IsOwner(PlanetMember member)
        {
            
            if (member == null) return false;
            if (member.UserId == Config.OwnerId) return true;
            return false;
        }
    }
}