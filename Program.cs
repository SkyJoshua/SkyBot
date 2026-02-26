using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using SkyBot;

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
if (!loginResult.Success)
{
    Console.WriteLine($"Login Failed: {loginResult.Message}");
    return;
}

await client.BotService.JoinAllChannelsAsync();

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

    string content = message.Content ?? "";
    long channelId = message.ChannelId;
    var member = await message.FetchAuthorMemberAsync();

    if (content is null) return;

    if (message.AuthorUserId == client.Me.Id) return;

    if (allowedUserIds.Contains(message.AuthorUserId))
    {
        if (Utils.IsSingleEmoji(content))
        {
            await message.AddReactionAsync(content);
        }
    }

    if (content.StartsWith("/sky echo"))
    {

        // if (message.AuthorUserId != 15652354820931584) await Utils.SendReplyAsync(channelCache, channelId, "You dont have permission to use this command.");

        var reply = content.Substring(10);

        if (reply is null) await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» Enter a message to echo.");

        reply = $"«@m-{member.Id}» {reply}";

        if (reply.Length > 2048)
        {
            reply = reply.Substring(0, 2048);
        }

        await Utils.SendReplyAsync(channelCache, channelId, reply);
    };

    if (Utils.ContainsAny(content, "/sky valourroadmap", "s/valourroadmap"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"«@m-{member.Id}» Here is the up-to-date roadmap for Valour:
                                                                        ~~1. Screen sharing~~
                                                                        2. Documentation
                                                                        3. Themes Improvements
                                                                        4. App stores
                                                                        5. Windows native bugs
                                                                        6. Tab system bugs
                                                                        7. Automod/planet moderation");
    }

    if (Utils.ContainsAny(content, "/sky suggestcommand", "/sky suggestcommands", "/sky suggest", "s/suggestcommand", "s/suggestcommands", "s/suggest"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» You can suggest a command to be added here: https://docs.google.com/spreadsheets/d/1CzcpLAuMiPL_RODrZ5x25cPj8yE-rR3mEnqrd_2Fbmk");
    };

    if (Utils.ContainsAny(content, "/sky source", "/sky github", "s/source", "s/github"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» You can see my source code here: https://github.com/SkyJoshua/SkyBot");
    };

    if (Utils.ContainsAny(content, "/sky test"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, "test");
    };
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);