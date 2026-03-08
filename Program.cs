using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using SkyBot;

Env.Load();

var token = Environment.GetEnvironmentVariable("TOKEN");
var allowedUserIds = new List<long> { 15652354820931584 };
var ownerId = 15652354820931584;
var prefix = "s/";

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

await Utils.UpdateValourUserCountAsync();
Utils.StartValourUserUpdater();


//Dictionaries
var channelCache = new Dictionary<long, Channel>();
var InitializedPlanets = new HashSet<long>(); 




await Utils.InitializePlanetsAsync(client, channelCache, InitializedPlanets);

client.PlanetService.JoinedPlanetsUpdated += async () =>
{
    await Utils.InitializePlanetsAsync(client, channelCache, InitializedPlanets);
};


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

    var echoprefixes = new[] { $"{prefix}echo"};
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

    var echorawprefixes = new[] { $"{prefix}rawecho"};
    if (Utils.ContainsAny(content, echorawprefixes))
    {

        if (message.AuthorUserId != ownerId)
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

    if (Utils.ContainsAny(content, $"{prefix}suggest"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} You can suggest a command to be added here: https://docs.google.com/spreadsheets/d/1CzcpLAuMiPL_RODrZ5x25cPj8yE-rR3mEnqrd_2Fbmk");
    };

    if (Utils.ContainsAny(content, $"{prefix}source"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} You can see my source code here: https://github.com/SkyJoshua/SkyBot");
    };

    if (Utils.ContainsAny(content, $"{prefix}joincode"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} You can use this to join a planet: https://github.com/SkyJoshua/JoinPlanet");
    };

    if (Utils.ContainsAny(content, $"{prefix}joinsite"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} You can use this website to easily add your bot to a planet: https://skyjoshua.xyz/planetjoiner");
    };

    if (Utils.ContainsAny(content, $"{prefix}api", $"{prefix}swagger"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"{pingMember} Here is a link to the Swagger API: https://api.valour.gg/swagger");
    };

    if (Utils.ContainsAny(content, $"{prefix}cmds"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember} Here is a list of my commands:
                                                                - `s/echo <text> - Echos text into the chat`
                                                                - `s/suggest - Shares the suggestions link`
                                                                - `s/source - Sends link for the source code`
                                                                - `s/joincode - Sends a link to a github that you can use to make your bot join your planet.`
                                                                - `s/joinsite - Sends a link to a website that you can use to make yout bot join your planet.`
                                                                - `s/api|swagger - Sends a link to the Swagger API`
                                                                - `s/cmds - Shows this list`
                                                                - `s/usercount - Shows the user count of Valour`
                                                                - `s/devcentral - Sends the invite link to the Dev Central Planet`
                                                                - `s/mc - Sends ValourSMP IP`
                                                                ");
    };

    if (Utils.ContainsAny(content, $"{prefix}usercount"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember}
                                                            Current Valour user count is: {Utils.ValourUserCount:N0}
                                                            You can see a graph of the user count here: /meow");
    };

    if (Utils.ContainsAny(content, $"{prefix}devcentral"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember} you can join the Dev Central (ID: 42439954653511681) planet here: https://app.valour.gg/I/k2tz9c4i");
    }

    if (Utils.ContainsAny(content, $"{prefix}mc"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, @$"{pingMember} you can join the Unofficial ValourSMP Minecraft Server by using this ip: 
                                                                Java: `valour.sxsc.xyz`, Bedrock: `valourbr.sxsc.xyz` Both with the default ports.
                                                                Cool features can be found here: https://sxsc.xyz/servers/valour/");
    }

    if (Utils.ContainsAny(content, $"{prefix}invite"))
    {
        if(message.AuthorUserId != ownerId) return;

        string[] args = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (args.Length < 2)
        {
            await Utils.SendReplyAsync(channelCache, channelId, "Usage: s/invite <planetId> [inviteCode]");
        }

        if (!long.TryParse(args[1], out long planetId))
        {
            await Utils.SendReplyAsync(channelCache, channelId, "Planet ID is not valid.");
            return;
        }

        string inviteCode = args.Length > 2 ? args[2] : "";

        var joinResult = await client.PlanetService.JoinPlanetAsync(planetId, inviteCode);

        if (joinResult.Success && joinResult.Data != null)
        {
            await Task.Delay(200);

            if (client.Cache.Planets.TryGet(planetId, out var planet))
            {
                if (planet is null) return;
                await Utils.SendReplyAsync(channelCache, channelId, $"Joined planet: {planet.Name}");
            }
            else
            {
                await Utils.SendReplyAsync(channelCache, channelId, "Joined planet, but could not retrieve its name.");
            }
        }
        else
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"Failed to join planet: {joinResult.Message}");
        }
    };
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);