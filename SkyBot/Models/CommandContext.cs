using System.Collections.Concurrent;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Models
{
    public class CommandContext
    {
        public required ValourClient Client{ get; set; }
        public required ConcurrentDictionary<long, Channel> ChannelCache { get; set; }
        public required PlanetMember Member { get; set; }
        public required Message Message { get; set; }
        public required Planet Planet { get; set; }
        public required long ChannelId { get; set; }
        public required string[] Args { get; set; }

    }
}