using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using SkyBot.Helpers;
using Valour.Sdk.Models;

namespace SkyBot.Services
{
    public static class WelcomeService
    {
        private static readonly ConcurrentDictionary<long, WelcomeConfig> _cache = new();

        public static async Task InitializeAsync()
        {
            using SqliteConnection connection = DatabaseHelper.GetConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM WelcomeConfigs";
            using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var config = new WelcomeConfig
                {
                    PlanetId = (long)reader["PlanetId"],
                    ChannelId = (long)reader["ChannelId"],
                    Message = (string)reader["Message"],
                    Active = (long)reader["Active"] == 1
                };
                _cache[config.PlanetId] = config;
            }
            Console.WriteLine("WelcomeService initialized.");
            Console.WriteLine($"Loaded {_cache.Count} welcome configs from database.");
        }

        public static async Task OnMemberJoin(PlanetMember member, ConcurrentDictionary<long, Channel> channelCache)
        {
            if (!_cache.TryGetValue(member.PlanetId, out var config)) { Console.WriteLine("No config found"); return; }
            if (!config.Active) { Console.WriteLine("Not active"); return; }

            Channel? channel = null;

            if (config.ChannelId != 0 && channelCache.TryGetValue(config.ChannelId, out var configChannel))
            {
                channel = configChannel;
            }
            else
            {
                channel = channelCache.Values.FirstOrDefault(c => c.PlanetId == member.PlanetId && c.IsDefault);
            }

            if (channel == null) { Console.WriteLine("No channel found"); return; }


            string message = config.Message
                .Replace("{username}", member.Name)
                .Replace("{fulluser}", member.User.NameAndTag)
                .Replace("{nickname}", string.IsNullOrWhiteSpace(member.Nickname) ? member.Name : member.Nickname)
                .Replace("{mention}", MessageHelper.Mention(member))
                .Replace("{id}", $"{member.Id}");

            await channel.SendMessageAsync(message);
        }

        public static async Task SetWelcomeChannel(long planetId, long channelId)
        {
            using SqliteConnection connection = DatabaseHelper.GetConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO WelcomeConfigs (PlanetId, ChannelId) VALUES ($planetId, $channelId)
                ON CONFLICT(PlanetId) DO UPDATE SET ChannelId = $channelId;
            ";
            cmd.Parameters.AddWithValue("$planetId", planetId);
            cmd.Parameters.AddWithValue("$channelId", channelId);
            await cmd.ExecuteNonQueryAsync();

            if (_cache.TryGetValue(planetId, out var config))
            {
                config.ChannelId = channelId;
            }
            else
            {
                _cache[planetId] = new WelcomeConfig{PlanetId = planetId, ChannelId = channelId};
            }
        }

        public static async Task SetWelcomeMessage(long planetId, string message)
        {
            using SqliteConnection connection = DatabaseHelper.GetConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO WelcomeConfigs (PlanetId, Message) VALUES ($planetId, $message)
                ON CONFLICT(PlanetId) DO UPDATE SET Message = $message;
            ";
            cmd.Parameters.AddWithValue("$planetId", planetId);
            cmd.Parameters.AddWithValue("$message", message);
            await cmd.ExecuteNonQueryAsync();

            if (_cache.TryGetValue(planetId, out var config))
            {
                config.Message = message;
            }
            else
            {
                _cache[planetId] = new WelcomeConfig{PlanetId = planetId, Message = message};
            }
        }

        public static async Task SetActive(long planetId, bool active)
        {
            using SqliteConnection connection = DatabaseHelper.GetConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO WelcomeConfigs (PlanetId, Active) VALUES ($planetId, $active)
                ON CONFLICT(PlanetId) DO UPDATE SET Active = $active;
            ";
            cmd.Parameters.AddWithValue("$planetId", planetId);
            cmd.Parameters.AddWithValue("$active", active ? 1 : 0);
            await cmd.ExecuteNonQueryAsync();

            if (_cache.TryGetValue(planetId, out var config))
            {
                config.Active = active;
            }
            else
            {
                _cache[planetId] = new WelcomeConfig{PlanetId = planetId, Active = active};
            }
        }

        public static async Task<bool?> SetActive(long planetId)
        {
            if (!_cache.TryGetValue(planetId, out var config)) return null;

            bool newActive = !config.Active;
            await SetActive(planetId, newActive);
            return newActive;
        }
    }
}