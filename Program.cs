using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using System.Globalization;

Env.Load();

var client = new ValourClient("https://api.valour.gg/");
client.SetupHttpClient();

var token = Environment.GetEnvironmentVariable("TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("TOKEN environment variable not set.");
    return;
}

var loginResult = await client.InitializeUser(token);
await client.BotService.JoinAllChannelsAsync();
if (!loginResult.Success)
{
    Console.WriteLine($"Login Failed: {loginResult.Message}");
    return;
}

var channelCache = new Dictionary<long, Channel>();

foreach (var planet in client.PlanetService.JoinedPlanets)
{
    foreach (var channel in planet.Channels)
    {
        channelCache[channel.Id] = channel;
        Console.WriteLine($"Cached: {channel.Id}");
    }
}


Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

var allowedUserIds = new List<long> { 15652354820931584 };

client.MessageService.MessageReceived += async (message) =>
{

    string content = message.Content;
    string lowerContent = content?.ToLower() ?? "";

    if (message.AuthorUserId == client.Me.Id) return;

    if (allowedUserIds.Contains(message.AuthorUserId))
    {
        if (IsSingleEmoji(message.Content))
        {
            await message.AddReactionAsync(message.Content);
        }
    }

    if (lowerContent.StartsWith("/sky echo "))
    {

        if (message.AuthorUserId != 15652354820931584)
        {
            if (channelCache.TryGetValue(message.ChannelId, out var chan))
            {
                await chan.SendMessageAsync("You dont have permission to use this command.");
                return;
            }
        }

        var reply = message.Content.Substring(10);

        if (channelCache.TryGetValue(message.ChannelId, out var channel))
        {
            await channel.SendMessageAsync(reply);
        }
        else
        {
            Console.WriteLine($"Channel {message.ChannelId} not found in cache.");
        }
    }

    if (lowerContent.Contains("/sky roadmap"))
    {
        if (channelCache.TryGetValue(message.ChannelId, out var channel))
        {
            var messageText = @"~~1. Screen sharing~~
                                2. Documentation
                                3. Themes Improvements
                                4. App stores
                                5. Windows native bugs
                                6. Tab system bugs
                                7. Automod/planet moderation";
            await channel.SendMessageAsync(messageText);
        }
    }

    if (lowerContent.Contains("/sky suggestcommands") || lowerContent.Contains("/sky suggestcommand"))
    {
        if (channelCache.TryGetValue(message.ChannelId, out var channel))
        {
            await channel.SendMessageAsync("You can suggest a command to be added here: https://docs.google.com/spreadsheets/d/1CzcpLAuMiPL_RODrZ5x25cPj8yE-rR3mEnqrd_2Fbmk");
        }
    }

    if (lowerContent.Contains("/sky source") || lowerContent.Contains("/sky github"))
    {
        if (channelCache.TryGetValue(message.ChannelId, out var channel))
        {
            await channel.SendMessageAsync("You can see the sourcecode for this bot here: https://github.com/SkyJoshua/SkyBot");
        }
    }
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);

static bool IsSingleEmoji(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        return false;

    input = input.Trim();

    var enumerator = StringInfo.GetTextElementEnumerator(input);
    int count = 0;

    while (enumerator.MoveNext())
        count++;

    return count == 1;
}