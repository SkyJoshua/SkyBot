using System.Collections.Concurrent;
using System.Text.Json;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;
using Valour.Shared.Authorization;

namespace SkyBot.Commands
{
    public class Wordle : ICommand
    {
        public string Name => "wordle";
        public string[] Aliases => ["wd"];
        public string Description => "Starts a channel-wide game of Wordle. Guess the 5-letter word in 6 tries!";
        public string Section => "Fun";
        public string Usage => "wordle | wordle board";

        private record WordleSession(
            string Word,
            List<string> Guesses,
            List<string[]> Feedback,
            HashSet<string> Contributors,
            long StarterId,
            Message BotMessage,
            Channel Channel,
            ValourClient Client);

        private static readonly ConcurrentDictionary<long, WordleSession> _sessions = new();
        private static readonly HttpClient _http = new();
        private static readonly SemaphoreSlim _fetchLock = new(1, 1);
        private static string[]? _cachedWords;

        private const int MaxGuesses = 6;
        private const int WordLength = 5;

        private static readonly string[] WordList =
        [
            "ABOUT", "ABUSE", "ACUTE", "ADMIT", "ADOPT", "AFTER", "AGENT", "AGREE",
            "AHEAD", "ALIKE", "ALIVE", "ALONE", "ALONG", "ALTER", "ANGEL", "ANGER",
            "ANGLE", "APART", "APPLE", "APPLY", "ARGUE", "ARISE", "ARMOR", "ASIDE",
            "ASSET", "AVOID", "AWARD", "AWARE", "BADLY", "BASIC", "BEACH", "BEARD",
            "BEGAN", "BEGIN", "BEING", "BELOW", "BENCH", "BLACK", "BLADE", "BLAME",
            "BLANK", "BLAST", "BLAZE", "BLEED", "BLESS", "BLIND", "BLOCK", "BLOOD",
            "BLOOM", "BLUNT", "BOARD", "BOOST", "BOUND", "BRAND", "BRAVE", "BREAD",
            "BREAK", "BREED", "BRICK", "BRIDE", "BRIEF", "BRING", "BROAD", "BROOK",
            "BROWN", "BRUSH", "BUILD", "BUILT", "BURST", "BUYER", "CABIN", "CABLE",
            "CANDY", "CARRY", "CHAIN", "CHAIR", "CHAOS", "CHARM", "CHEAP", "CHECK",
            "CHESS", "CHEST", "CHIEF", "CHILD", "CHILL", "CIVIC", "CIVIL", "CLAIM",
            "CLASS", "CLEAN", "CLEAR", "CLIMB", "CLOCK", "CLOSE", "CLOUD", "COACH",
            "COAST", "COUNT", "COURT", "COVER", "CRACK", "CRAFT", "CRANE", "CRAZY",
            "CREAM", "CREEK", "CRIME", "CROSS", "CROWD", "CRUSH", "CURVE", "CYCLE",
            "DAILY", "DANCE", "DEATH", "DEBUT", "DENSE", "DEPOT", "DEPTH", "DERBY",
            "DINER", "DIRTY", "DISCO", "DITCH", "DIZZY", "DOUBT", "DOUGH", "DRAFT",
            "DRAIN", "DRAMA", "DRANK", "DREAM", "DRESS", "DRIFT", "DRINK", "DRIVE",
            "DRONE", "DROVE", "DROWN", "DRUNK", "DYING", "EAGER", "EAGLE", "EARLY",
            "EARTH", "EIGHT", "ELITE", "EMPTY", "ENEMY", "ENJOY", "ENTER", "ENTRY",
            "EQUAL", "ERROR", "ESSAY", "EVENT", "EVERY", "EXACT", "EXIST", "EXTRA",
            "FABLE", "FAINT", "FAITH", "FALSE", "FANCY", "FATAL", "FAULT", "FEAST",
            "FENCE", "FEVER", "FIELD", "FIERY", "FIFTH", "FIFTY", "FIGHT", "FINAL",
            "FIRST", "FIXED", "FLAME", "FLASH", "FLEET", "FLESH", "FLOAT", "FLOCK",
            "FLOOD", "FLOOR", "FLUSH", "FOCUS", "FORCE", "FORGE", "FORTH", "FOUND",
            "FRAME", "FRANK", "FRAUD", "FRESH", "FRONT", "FROST", "FRUIT", "FULLY",
            "FUNNY", "GIANT", "GIVEN", "GLASS", "GLEAM", "GLOBE", "GLOOM", "GLORY",
            "GLOVE", "GOING", "GRACE", "GRADE", "GRAIN", "GRAND", "GRANT", "GRAPE",
            "GRASP", "GRASS", "GRAVE", "GREAT", "GREEN", "GREET", "GRIEF", "GRIND",
            "GROAN", "GROOM", "GROSS", "GROUP", "GROVE", "GROWN", "GUARD", "GUESS",
            "GUIDE", "GUILT", "GUISE", "HARSH", "HEART", "HEAVY", "HENCE", "HERBS",
            "HINGE", "HONEY", "HONOR", "HORSE", "HOTEL", "HOUSE", "HUMAN", "HURRY",
            "IDEAL", "IMAGE", "INNER", "INPUT", "ISSUE", "IVORY", "JEWEL", "JOINT",
            "JUDGE", "JUICE", "JUICY", "JUMBO", "KARMA", "KNACK", "KNEEL", "KNIFE",
            "KNOCK", "KNOWN", "LABEL", "LANCE", "LARGE", "LASER", "LATER", "LAUGH",
            "LAYER", "LEARN", "LEASE", "LEAST", "LEGAL", "LEMON", "LEVEL", "LIGHT",
            "LIMIT", "LIVER", "LOCAL", "LODGE", "LOGIC", "LOOSE", "LOVER", "LOWER",
            "LUCKY", "LUNAR", "MAGIC", "MAJOR", "MAKER", "MANOR", "MAPLE", "MARCH",
            "MATCH", "MAYOR", "MEDIA", "MERCY", "MERIT", "METAL", "MIGHT", "MINOR",
            "MINUS", "MODEL", "MONEY", "MONTH", "MORAL", "MOTOR", "MOUNT", "MOUSE",
            "MOUTH", "MOVIE", "MUSIC", "NAIVE", "NERVE", "NEVER", "NIGHT", "NINJA",
            "NOBLE", "NOISE", "NORTH", "NOVEL", "NURSE", "NYMPH", "OCEAN", "OFFER",
            "OFTEN", "OLIVE", "ONSET", "OPERA", "ORDER", "OTHER", "OUTER", "OWNED",
            "OWNER", "OZONE", "PAINT", "PANIC", "PAPER", "PARTY", "PEACE", "PEACH",
            "PEARL", "PENNY", "PHASE", "PHONE", "PHOTO", "PIANO", "PIECE", "PILOT",
            "PIXEL", "PLACE", "PLAIN", "PLANE", "PLANT", "PLATE", "PLAZA", "PLEAD",
            "PLUMB", "PLUMP", "POINT", "POLAR", "POPPY", "POWER", "PRESS", "PRICE",
            "PRIDE", "PRIME", "PRINT", "PRIOR", "PRISM", "PROBE", "PROOF", "PROSE",
            "PROUD", "PROVE", "PULSE", "PUPIL", "QUEEN", "QUERY", "QUEST", "QUEUE",
            "QUIET", "QUOTA", "QUOTE", "RADAR", "RADIO", "RAISE", "RALLY", "RANGE",
            "RAPID", "RATIO", "REACH", "READY", "REALM", "REBEL", "REFER", "REIGN",
            "RELAX", "REPLY", "RIDER", "RIDGE", "RISKY", "RIVER", "ROBIN", "ROBOT",
            "ROCKY", "ROUGH", "ROUND", "ROUTE", "ROYAL", "RULER", "RURAL", "SADLY",
            "SAINT", "SALAD", "SAUCE", "SCALE", "SCENE", "SCENT", "SCOUT", "SENSE",
            "SEVEN", "SHADE", "SHAFT", "SHALL", "SHAME", "SHAPE", "SHARE", "SHARK",
            "SHARP", "SHEEP", "SHEER", "SHELF", "SHELL", "SHIFT", "SHINE", "SHIRT",
            "SHOCK", "SHOOT", "SHORT", "SHOUT", "SIEGE", "SIGHT", "SILLY", "SINCE",
            "SIXTH", "SIXTY", "SKILL", "SKULL", "SLATE", "SLAVE", "SLEEP", "SLICE",
            "SLIDE", "SLOPE", "SMALL", "SMART", "SMELL", "SMILE", "SMOKE", "SNAKE",
            "SOLAR", "SOLID", "SOLVE", "SORRY", "SOUND", "SOUTH", "SPACE", "SPARE",
            "SPARK", "SPEAK", "SPEAR", "SPEED", "SPEND", "SPICE", "SPINE", "SPITE",
            "SPLIT", "SPOKE", "SPOON", "SPORT", "SPRAY", "SQUAD", "STACK", "STAFF",
            "STAGE", "STAIN", "STAIR", "STAKE", "STAND", "STARK", "STATE", "STEAM",
            "STEEL", "STEEP", "STEER", "STERN", "STICK", "STIFF", "STILL", "STOCK",
            "STOMP", "STONE", "STORE", "STORM", "STORY", "STRAP", "STRAW", "STRAY",
            "STRIP", "STUCK", "STUDY", "STUFF", "STYLE", "SUGAR", "SUITE", "SUNNY",
            "SUPER", "SURGE", "SWAMP", "SWEAR", "SWEEP", "SWEET", "SWIFT", "SWIPE",
            "SWORD", "TABLE", "TASTE", "TEACH", "TEARS", "TEMPT", "TENSE", "TENTH",
            "THEFT", "THEIR", "THERE", "THICK", "THING", "THINK", "THIRD", "THREE",
            "THREW", "THROW", "TIGER", "TIGHT", "TIMER", "TIRED", "TITAN", "TITLE",
            "TOKEN", "TOPIC", "TOTAL", "TOUCH", "TOUGH", "TOWER", "TOXIC", "TRACE",
            "TRACK", "TRADE", "TRAIL", "TRAIN", "TRAIT", "TRASH", "TREND", "TRIAL",
            "TRIBE", "TRICK", "TRIED", "TROOP", "TROUT", "TRUCK", "TRULY", "TRUNK",
            "TRUST", "TRUTH", "TWIST", "ULTRA", "UNCLE", "UNDER", "UNION", "UNITY",
            "UNTIL", "UPPER", "UPSET", "URBAN", "USAGE", "USHER", "USUAL", "UTTER",
            "VAGUE", "VALID", "VALUE", "VAPOR", "VAULT", "VIGOR", "VIRAL", "VISIT",
            "VITAL", "VIVID", "VOCAL", "VOICE", "VOTER", "WAGER", "WATCH", "WATER",
            "WEARY", "WEAVE", "WEDGE", "WEIRD", "WHERE", "WHILE", "WHITE", "WHOLE",
            "WIDER", "WITCH", "WOMAN", "WOMEN", "WORLD", "WORRY", "WORSE", "WORST",
            "WORTH", "WOULD", "WRATH", "WRITE", "WRONG", "YACHT", "YIELD", "YOUNG",
            "YOUTH", "ZEBRA",
        ];

        private static async Task<string[]> GetWordPoolAsync()
        {
            if (_cachedWords is not null) return _cachedWords;

            await _fetchLock.WaitAsync();
            try
            {
                if (_cachedWords is not null) return _cachedWords;

                string json = await _http.GetStringAsync(
                    "https://api.datamuse.com/words?sp=?????&max=1000&md=f");

                using var doc = JsonDocument.Parse(json);
                var words = doc.RootElement.EnumerateArray()
                    .Select(e => e.GetProperty("word").GetString() ?? "")
                    .Where(w => w.Length == WordLength && w.All(char.IsLetter))
                    .Select(w => w.ToUpper())
                    .ToArray();

                if (words.Length > 0)
                {
                    _cachedWords = words;
                    return words;
                }
            }
            catch { }
            finally
            {
                _fetchLock.Release();
            }

            return WordList;
        }

        private static async Task<bool> IsValidWordAsync(string word)
        {
            try
            {
                string json = await _http.GetStringAsync(
                    $"https://api.datamuse.com/words?sp={word.ToLower()}&max=1");

                using var doc = JsonDocument.Parse(json);
                var arr = doc.RootElement;
                if (arr.GetArrayLength() > 0)
                {
                    string? returned = arr[0].GetProperty("word").GetString();
                    return string.Equals(returned, word, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }
            return false;
        }

        // Returns per-letter emoji feedback using standard Wordle rules
        private static string[] GetFeedback(string guess, string word)
        {
            var result   = new string[WordLength];
            var wordLeft = word.ToCharArray();
            var guessLeft = guess.ToCharArray();

            // First pass: greens
            for (int i = 0; i < WordLength; i++)
            {
                if (guessLeft[i] == wordLeft[i])
                {
                    result[i]    = "🟩";
                    wordLeft[i]  = '\0';
                    guessLeft[i] = '\0';
                }
            }

            // Second pass: yellows and grays
            for (int i = 0; i < WordLength; i++)
            {
                if (guessLeft[i] == '\0') continue;

                int idx = Array.IndexOf(wordLeft, guessLeft[i]);
                if (idx >= 0)
                {
                    result[i]    = "🟨";
                    wordLeft[idx] = '\0';
                }
                else
                {
                    result[i] = "⬛";
                }
            }

            return result;
        }

        private static string BuildDisplay(WordleSession session)
        {
            var rows = new List<string>();

            for (int i = 0; i < MaxGuesses; i++)
            {
                if (i < session.Guesses.Count)
                {
                    string emojis  = string.Join("", session.Feedback[i]);
                    string letters = string.Join(" ", session.Guesses[i].ToCharArray());
                    rows.Add($"{emojis}  {letters}");
                }
                else
                {
                    rows.Add("⬜⬜⬜⬜⬜");
                }
            }

            return string.Join("\n",
                "🟩 **WORDLE** — Guess the 5-letter word!",
                "",
                string.Join("\n", rows),
                "",
                $"Guesses: {session.Guesses.Count}/{MaxGuesses}",
                "",
                $"*Use `{Config.Prefix}wg <word>` to guess!*"
            );
        }

        public static async Task ProcessGuessAsync(CommandContext ctx, Channel channel, string rawGuess)
        {
            long channelId = ctx.ChannelId;
            if (!_sessions.TryGetValue(channelId, out var session)) return;

            string guess      = rawGuess.ToUpper();
            string memberName = ctx.Member.Name ?? "Unknown";

            if (guess.Length != WordLength || !guess.All(char.IsLetter))
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"Your guess must be exactly {WordLength} letters with no numbers or symbols.");
                return;
            }

            if (!await IsValidWordAsync(guess))
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"`{guess}` isn't a valid word!");
                return;
            }

            if (session.Guesses.Contains(guess))
            {
                await MessageHelper.ReplyAsync(ctx, channel, $"`{guess}` has already been guessed!");
                return;
            }

            var feedback = GetFeedback(guess, session.Word);
            session.Guesses.Add(guess);
            session.Feedback.Add(feedback);
            session.Contributors.Add(memberName);

            bool won  = guess == session.Word;
            bool lost = !won && session.Guesses.Count >= MaxGuesses;

            if (won)
            {
                _sessions.TryRemove(channelId, out _);
                string contributorList = string.Join(", ", session.Contributors);
                await RepostBoardAsync(ctx, channel, session,
                    BuildDisplay(session)
                    + $"\n\n🎉 **{memberName} got it in {session.Guesses.Count}!**\nContributors: {contributorList}");
                await MessageHelper.ReplyAsync(ctx, channel, $"🎉 **The word was `{session.Word}`!** Got it in {session.Guesses.Count}/{MaxGuesses}!");
            }
            else if (lost)
            {
                _sessions.TryRemove(channelId, out _);
                await RepostBoardAsync(ctx, channel, session,
                    BuildDisplay(session)
                    + $"\n\n💀 **Game over!** The word was `{session.Word}`.");
                await MessageHelper.ReplyAsync(ctx, channel, $"💀 **Game over!** The word was `{session.Word}`.");
            }
            else
            {
                var newMsg = await RepostBoardAsync(ctx, channel, session, BuildDisplay(session));
                if (newMsg is not null)
                    _sessions[channelId] = session with { BotMessage = newMsg };
            }
        }

        private static async Task<Message?> RepostBoardAsync(CommandContext ctx, Channel channel, WordleSession session, string content)
        {
            if (ctx.Client.Cache.Messages.TryGet(session.BotMessage.Id, out var old) && old is not null)
                try { await old.DeleteAsync(); } catch { }
            else
                try { await session.BotMessage.DeleteAsync(); } catch { }

            var result = await MessageHelper.ReplyAsync(ctx, channel, content);
            if (!result.Success || result.Data is null) return null;
            return ctx.Client.Cache.Messages.TryGet(result.Data.Id, out var cached) && cached is not null ? cached : result.Data;
        }

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            string[] args = ctx.Args;
            PlanetMember member = ctx.Member;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            // sd/wordle end — end the current game
            if (args.Length >= 1 && args[0].ToLower() == "end")
            {
                if (!_sessions.TryGetValue(channelId, out var session))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "There's no active Wordle game in this channel.");
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
                if (ctx.Client.Cache.Messages.TryGet(session.BotMessage.Id, out var old) && old is not null)
                    try { await old.DeleteAsync(); } catch { }
                else
                    try { await session.BotMessage.DeleteAsync(); } catch { }
                await MessageHelper.ReplyAsync(ctx, channel, $"🛑 Wordle ended by {member.Name}. The word was `{session.Word}`.");
                return;
            }

            // sd/wordle board — repost the current board
            if (args.Length >= 1 && args[0].ToLower() == "board")
            {
                if (!_sessions.TryGetValue(channelId, out var session))
                {
                    await MessageHelper.ReplyAsync(ctx, channel, "There's no active Wordle game in this channel.");
                    return;
                }

                var newMsg = await RepostBoardAsync(ctx, channel, session, BuildDisplay(session));
                if (newMsg is not null)
                    _sessions[channelId] = session with { BotMessage = newMsg };
                return;
            }

            // sd/wordle — start a new game
            if (_sessions.ContainsKey(channelId))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "There's already an active Wordle game in this channel!");
                return;
            }

            var pool = await GetWordPoolAsync();
            string word = pool[Random.Shared.Next(pool.Length)];

            var newSession = new WordleSession(word, [], [], [], member.UserId, null!, channel, ctx.Client);
            string display = BuildDisplay(newSession);
            var sent = await MessageHelper.ReplyAsync(ctx, channel, display);
            if (!sent.Success || sent.Data is null) return;

            _sessions[channelId] = newSession with { BotMessage = sent.Data };
        }
    }

    public class WordleGuess : ICommand
    {
        public string Name => "wg";
        public string[] Aliases => [];
        public string Description => "Guess a word in the active Wordle game.";
        public string Section => "Fun";
        public string Usage => "wg <word>";

        public async Task Execute(CommandContext ctx)
        {
            if (!ctx.ChannelCache.TryGetValue(ctx.ChannelId, out var channel)) return;

            if (ctx.Args.Length == 0 || string.IsNullOrWhiteSpace(ctx.Args[0]))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please provide a word to guess.");
                return;
            }

            await Wordle.ProcessGuessAsync(ctx, channel, ctx.Args[0]);
        }
    }
}
