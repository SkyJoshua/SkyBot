using System.Collections.Concurrent;
using System.Text.Json;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Commands
{
    public class Hangman : ICommand
    {
        public string Name => "hangman";
        public string[] Aliases => ["hm"];
        public string Description => "Starts a channel-wide game of hangman. Optionally specify a category.";
        public string Section => "Fun";
        public string Usage => "hangman [category] | hangman end";

        private record HangmanSession(
            string Word,
            string? Category,
            HashSet<char> Guessed,
            HashSet<char> Wrong,
            HashSet<string> Contributors,
            long StarterId,
            Message BotMessage,
            Channel Channel,
            ValourClient Client);

        private static readonly ConcurrentDictionary<long, HangmanSession> _sessions = new();
        private static readonly HttpClient _http = new();

        private const int MaxWrong = 6;

        private static readonly string[] Topics =
        [
            "animals", "food", "sports", "music", "science", "geography",
            "movies", "nature", "technology", "history", "mythology", "space",
            "ocean", "weather", "games", "art", "clothing", "vehicles",
        ];

        private static readonly string[] FallbackWords =
        [
            "APPLE", "BRIDGE", "CASTLE", "DRAGON", "ELEPHANT", "FOREST", "GUITAR",
            "HARBOR", "ISLAND", "JUNGLE", "KNIGHT", "LEMON", "MANGO", "OCEAN",
            "PLANET", "ROBOT", "SNAKE", "TIGER", "UMBRELLA", "WIZARD", "ANCHOR",
            "BUTTER", "CANDLE", "DONKEY", "ENGINE", "FALCON", "GOBLIN", "HAMMER",
            "IGLOO", "JACKET", "KITTEN", "LADDER", "MIRROR", "NEEDLE", "ORANGE",
            "PENCIL", "RABBIT", "SILVER", "TEMPLE", "TURTLE", "VALLEY", "WALRUS",
            "ZIPPER", "BLANKET", "CACTUS", "DAISY", "GLOVES", "HOCKEY", "INSECT",
            "JELLY", "KETTLE", "LOBSTER", "MARBLE", "NAPKIN", "OYSTER", "PEPPER",
            "QUARTZ", "ROCKET", "SALMON", "THRONE", "VELVET", "WINDOW", "YOGURT",
            "ZOMBIE", "ALMOND", "BISON", "COBRA", "DAGGER", "EMBER", "FROST",
            "GHOST", "HONEY", "IVORY", "JEWEL", "KOALA", "MAPLE", "NINJA",
            "OLIVE", "PIRATE", "RAVEN", "SPHINX", "TORNADO", "UNICORN", "VENOM",
            "WITCH", "PIXEL", "STORM", "CLOUD", "FLAME", "COMET", "DUSK",
            "ECHO", "FABLE", "GLYPH", "HAZE", "JINX", "KNACK", "LUNAR",
            "MYTH", "NEON", "ORBIT", "PRISM", "QUEST", "RIDGE", "SHARD",
        ];

        // 7 stages: 0 wrong → 6 wrong (using +--+ to avoid markdown eating underscores)
        private static readonly string[] Stages =
        [
            "```\n  +--------+\n  |        |\n  |         \n  |         \n  |         \n  |         \n==+==\n```",
            "```\n  +--------+\n  |        |\n  |        O\n  |         \n  |         \n  |         \n==+==\n```",
            "```\n  +--------+\n  |        |\n  |        O\n  |        |\n  |         \n  |         \n==+==\n```",
            "```\n  +--------+\n  |        |\n  |        O\n  |       /|\n  |         \n  |         \n==+==\n```",
            "```\n  +--------+\n  |        |\n  |        O\n  |       /|\\\n  |         \n  |         \n==+==\n```",
            "```\n  +--------+\n  |        |\n  |        O\n  |       /|\\\n  |       /  \n  |         \n==+==\n```",
            "```\n  +--------+\n  |        |\n  |        O\n  |       /|\\\n  |       / \\\n  |         \n==+==\n```",
        ];

        private static async Task DeleteBotMessageAsync(CommandContext ctx, Message botMessage)
        {
            if (ctx.Client.Cache.Messages.TryGet(botMessage.Id, out var cached) && cached is not null)
                try { await cached.DeleteAsync(); } catch { }
            else
                try { await botMessage.DeleteAsync(); } catch { }
        }

        private static async Task<Message?> RepostAsync(CommandContext ctx, Channel channel, Message botMessage, string content)
        {
            await DeleteBotMessageAsync(ctx, botMessage);
            var result = await MessageHelper.ReplyAsync(ctx, channel, content);
            if (!result.Success || result.Data is null) return null;
            return ctx.Client.Cache.Messages.TryGet(result.Data.Id, out var cached) ? cached : result.Data;
        }

        private static async Task<string> FetchWord(string topic)
        {
            try
            {
                string url = $"https://api.datamuse.com/words?ml={Uri.EscapeDataString(topic)}&topic={Uri.EscapeDataString(topic)}&max=500";
                string json = await _http.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var words = doc.RootElement.EnumerateArray()
                    .Select(e => e.GetProperty("word").GetString() ?? "")
                    .Where(w => w.Length >= 4 && w.Length <= 12 && w.All(char.IsLetter))
                    .ToList();

                if (words.Count > 0)
                    return words[Random.Shared.Next(words.Count)].ToUpper();
            }
            catch { }

            return FallbackWords[Random.Shared.Next(FallbackWords.Length)];
        }

        private static string BuildDisplay(string word, string? category, HashSet<char> guessed, HashSet<char> wrong)
        {
            string wordDisplay = string.Join(" ", word.Select(c => guessed.Contains(c) ? c.ToString() : "_"));
            string wrongLetters = wrong.Count > 0 ? string.Join(", ", wrong.OrderBy(c => c)) : "none";
            string categoryLine = category is not null ? $"📂 **Category**: {category.ToTitleCase()}\n" : "";

            return string.Join("\n",
                $"🎮 **HANGMAN**",
                categoryLine,
                $"`{wordDisplay}`",
                "",
                Stages[wrong.Count],
                "",
                $"❌ Wrong ({wrong.Count}/{MaxWrong}): {wrongLetters}",
                "",
                $"*Use `{Config.Prefix}hg <letter or word>` to guess!*"
            );
        }

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            Message message = ctx.Message;
            string[] args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            // sd/hangman end — end the current game
            if (args.Length >= 1 && args[0].ToLower() == "end")
            {
                if (!_sessions.TryGetValue(channelId, out var session))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "There's no active hangman game in this channel.");
                    return;
                }

                bool isStarter = member.UserId == session.StarterId;
                bool isMod     = await PermissionHelper.HasPermAsync(member, channel, [ChatChannelPermissions.ManageMessages]);

                if (!isStarter && !isMod)
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "Only the person who started the game (or a moderator) can end it.");
                    return;
                }

                _sessions.TryRemove(channelId, out _);
                await DeleteBotMessageAsync(ctx, session.BotMessage);
                await MessageHelper.ReplyAsync(ctx, channel, $"🛑 Hangman ended by {member.Name}. The word was `{session.Word}`.");
                return;
            }

            // sd/hangman [category] — start a new game
            if (_sessions.ContainsKey(channelId))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "There's already an active hangman game in this channel!");
                return;
            }

            string? category = args.Length >= 1
                ? args[0].ToLower()
                : Topics[Random.Shared.Next(Topics.Length)];

            string word = await FetchWord(category);

            var guessed = new HashSet<char>();
            var wrong   = new HashSet<char>();
            var contributors = new HashSet<string>();

            string display = BuildDisplay(word, category, guessed, wrong);
            var sent = await MessageHelper.ReplyAsync(ctx, channel, display);
            if (!sent.Success || sent.Data is null) return;

            _sessions[channelId] = new HangmanSession(word, category, guessed, wrong, contributors, member.UserId, sent.Data, channel, ctx.Client);
        }

        public static async Task ProcessGuessAsync(CommandContext ctx, Channel channel, string rawGuess)
        {
            long channelId = ctx.ChannelId;

            if (!_sessions.TryGetValue(channelId, out var session))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "There's no active hangman game in this channel.");
                return;
            }

            string guess = rawGuess.ToUpper();
            string memberName = ctx.Member.Name ?? "Unknown";

            // Full word guess
            if (guess.Length > 1)
            {
                if (guess == session.Word)
                {
                    foreach (char c in session.Word) session.Guessed.Add(c);
                    session.Contributors.Add(memberName);
                    _sessions.TryRemove(channelId, out _);

                    string contributorList = string.Join(", ", session.Contributors);
                    await RepostAsync(ctx, channel, session.BotMessage,
                        BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong)
                        + $"\n\n🎉 **{memberName} guessed the word! The word was `{session.Word}`!**\nContributors: {contributorList}");
                    await MessageHelper.ReplyAsync(ctx, channel, $"🎉 **{memberName} guessed the word!** The word was `{session.Word}`!");
                }
                else
                {
                    session.Wrong.Add(guess[0]);

                    if (session.Wrong.Count >= MaxWrong)
                    {
                        _sessions.TryRemove(channelId, out _);
                        await RepostAsync(ctx, channel, session.BotMessage,
                            BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong)
                            + $"\n\n💀 **Game over!** The word was `{session.Word}`.");
                        await MessageHelper.ReplyAsync(ctx, channel, $"💀 **Game Over!** Out of guesses. The word was `{session.Word}`");
                    }
                    else
                    {
                        var newMsg = await RepostAsync(ctx, channel, session.BotMessage,
                            BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong));
                        if (newMsg is not null)
                            _sessions[channelId] = session with { BotMessage = newMsg };
                        await MessageHelper.ReplyAsync(ctx, channel, $"❌ `{guess}` is not the word!");
                    }
                }
                return;
            }

            // Single letter guess
            char letter = guess[0];

            if (!char.IsLetter(letter))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please guess a letter or a full word.");
                return;
            }

            if (session.Guessed.Contains(letter) || session.Wrong.Contains(letter))
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"`{letter}` has already been guessed!");
                return;
            }

            if (session.Word.Contains(letter))
            {
                session.Guessed.Add(letter);
                session.Contributors.Add(memberName);

                bool won = session.Word.All(c => session.Guessed.Contains(c));
                if (won)
                {
                    _sessions.TryRemove(channelId, out _);
                    string contributorList = string.Join(", ", session.Contributors);
                    await RepostAsync(ctx, channel, session.BotMessage,
                        BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong)
                        + $"\n\n🎉 **The channel wins! The word was `{session.Word}`!**\nContributors: {contributorList}");
                    await MessageHelper.ReplyAsync(ctx, channel, $"🎉 **The channel wins!** The word was `{session.Word}`!");
                }
                else
                {
                    var newMsg = await RepostAsync(ctx, channel, session.BotMessage,
                        BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong));
                    if (newMsg is not null)
                        _sessions[channelId] = session with { BotMessage = newMsg };
                    await MessageHelper.ReplyAsync(ctx, channel, $"✅ `{letter}` is in the word!");
                }
            }
            else
            {
                session.Wrong.Add(letter);

                if (session.Wrong.Count >= MaxWrong)
                {
                    _sessions.TryRemove(channelId, out _);
                    await RepostAsync(ctx, channel, session.BotMessage,
                        BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong)
                        + $"\n\n💀 **Game over!** The word was `{session.Word}`.");
                    await MessageHelper.ReplyAsync(ctx, channel, $"💀 **Game over!** The word was `{session.Word}`.");
                }
                else
                {
                    var newMsg = await RepostAsync(ctx, channel, session.BotMessage,
                        BuildDisplay(session.Word, session.Category, session.Guessed, session.Wrong));
                    if (newMsg is not null)
                        _sessions[channelId] = session with { BotMessage = newMsg };
                    await MessageHelper.ReplyAsync(ctx, channel, $"❌ No `{letter}` in the word!");
                }
            }
        }
    }

    public class HangmanGuess : ICommand
    {
        public string Name => "hg";
        public string[] Aliases => [];
        public string Description => "Guess a letter or word in the active Hangman game.";
        public string Section => "Fun";
        public string Usage => "hg <letter or word>";

        public async Task Execute(CommandContext ctx)
        {
            if (!ctx.ChannelCache.TryGetValue(ctx.ChannelId, out var channel)) return;

            if (ctx.Args.Length == 0 || string.IsNullOrWhiteSpace(ctx.Args[0]))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please provide a letter or word to guess.");
                return;
            }

            await Hangman.ProcessGuessAsync(ctx, channel, ctx.Args[0]);
        }
    }
}
