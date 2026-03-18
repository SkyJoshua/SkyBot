using Microsoft.Data.Sqlite;

namespace SkyBot.Helpers
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=database.db";

        public static SqliteConnection GetConnection()
        {
            SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public static async Task InitializeAsync()
        {
            using SqliteConnection connection = GetConnection();
            using SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS WelcomeConfigs (
                    PlanetId INTEGER PRIMARY KEY,
                    ChannelId INTEGER NOT NULL DEFAULT 0,
                    Message TEXT NOT NULL DEFAULT 'Welcome to the planet, {username}!',
                    Active INTEGER NOT NULL DEFAULT 0
                );
            ";
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Database initialized.");
        }
    }
}