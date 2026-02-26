using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using SkyBot;

Env.Load();

var token = Environment.GetEnvironmentVariable("TOKEN");
var channelCache = new Dictionary<long, Channel>();
var allowedUserIds = new List<long> { 15652354820931584 };

var client = new ValourClient("https://api.valour.gg/");
client.SetupHttpClient();

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
Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

await client.BotService.JoinAllChannelsAsync();
foreach (var planet in client.PlanetService.JoinedPlanets)
{
    foreach (var channel in planet.Channels)
    {
        channelCache[channel.Id] = channel;
        Console.WriteLine($"Cached: {channel.Id}");
    }
}

await Utils.UpdateValourUserCountAsync();
Utils.StartValourUserUpdater();

client.MessageService.MessageReceived += async (message) =>
{
    string content = message.Content ?? "";
    long channelId = message.ChannelId;
    var member = await message.FetchAuthorMemberAsync();
    var pingMember = $"«@m-{member.Id}»";

    if (content is null) return;

    if (message.AuthorUserId == client.Me.Id) return;


    if (allowedUserIds.Contains(message.AuthorUserId))
    {
        if (Utils.IsSingleEmoji(content))
        {
            await message.AddReactionAsync(content);
        }
    };

    var echoprefixes = new[] { "/sky echo", "s/echo"};
    if (Utils.ContainsAny(content, echoprefixes))
    {

        var matchedPrefix = echoprefixes.First(p => content.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        var reply = content.Substring(matchedPrefix.Length).TrimStart();

        if (string.IsNullOrWhiteSpace(reply)) await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} Enter a message to echo.");

        reply = $"{pingMember} {reply}";

        if (reply.Length > 2048)
        {
            reply = reply.Substring(0, 2048);
        }

        await Utils.SendReplyAsync(channelCache, channelId, reply);
    };

    var echorawprefixes = new[] { "/sky rawecho", "s/rawecho"};
    if (Utils.ContainsAny(content, echorawprefixes))
    {

        if (message.AuthorUserId != 15652354820931584)
        {
            await Utils.SendReplyAsync(channelCache, channelId, "You do not have permission to execute this command.");
            return;
        }
        
        var matchedPrefix = echorawprefixes.First(p => content.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        var reply = content.Substring(matchedPrefix.Length).TrimStart();

        if (string.IsNullOrWhiteSpace(reply)) 
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} Enter a message to echo.");
            return;
        }

        reply = $"{reply}";

        if (reply.Length > 2048)
        {
            reply = reply.Substring(0, 2048);
        }

        await Utils.SendReplyAsync(channelCache, channelId, reply);
    };

    if (Utils.ContainsAny(content, "/sky valourroadmap", "s/valourroadmap"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember} Here is the up-to-date roadmap for Valour:
                                                                        ~~1. Screen sharing~~
                                                                        2. Documentation
                                                                        3. Themes Improvements
                                                                        4. App stores
                                                                        5. Windows native bugs
                                                                        6. Tab system bugs
                                                                        7. Automod/planet moderation");
    };

    if (Utils.ContainsAny(content, "/sky suggestcommand", "/sky suggest", "s/suggestcommand", "s/suggest"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} You can suggest a command to be added here: https://docs.google.com/spreadsheets/d/1CzcpLAuMiPL_RODrZ5x25cPj8yE-rR3mEnqrd_2Fbmk");
    };

    if (Utils.ContainsAny(content, "/sky source", "/sky github", "s/source", "s/github"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} You can see my source code here: https://github.com/SkyJoshua/SkyBot");
    };

    if (Utils.ContainsAny(content, "/sky commands", "/sky cmds", "s/commands", "s/cmds"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember} Here is a list of my commands:
                                                                prefixes can be `/sky` or `s/`
                                                                `/sky echo <text>`
                                                                `/sky valourroadmap`
                                                                `/sky suggest|suggestcommand`
                                                                `/sky source|github`
                                                                `/sky commands|cmds`
                                                                `/sky usercount`");
    };

    if (Utils.ContainsAny(content, "/sky usercount", "s/usercount"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember}
                                                            Current Valour user count is: {Utils.ValourUserCount:N0}
                                                            You can see a graph of the user count here: /meow");
    };
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);