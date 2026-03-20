using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using SkyBot.Helpers;
using SkyBot.Models;
using Valour.Sdk.Client;
using Valour.Sdk.Models;

namespace SkyBot.Commands
{
    public class Trivia : ICommand
    {
        public string Name => "trivia";
        public string[] Aliases => ["triv"];
        public string Description => "Starts a channel-wide trivia question. Everyone has 30 seconds to guess.";
        public string Section => "Fun";
        public string Usage => "trivia [easy|medium|hard] [topic] | trivia topics";

        private record GuessEntry(char Letter, string MemberName);

        private record TriviaSession(
            char CorrectLetter,
            string QuestionText,
            List<string> Answers,
            Message BotMessage,
            Channel Channel,
            ValourClient Client,
            ConcurrentDictionary<long, GuessEntry> Guesses);

        private static readonly ConcurrentDictionary<long, TriviaSession> _sessions = new();
        private static readonly HttpClient _http = new();

        private static readonly string[] Difficulties = ["easy", "medium", "hard"];

        private static readonly Dictionary<string, int> Categories = new()
        {
            ["general"]     = 9,
            ["books"]       = 10,
            ["film"]        = 11,
            ["movies"]      = 11,
            ["music"]       = 12,
            ["musicals"]    = 13,
            ["tv"]          = 14,
            ["television"]  = 14,
            ["games"]       = 15,
            ["videogames"]  = 15,
            ["boardgames"]  = 16,
            ["science"]     = 17,
            ["nature"]      = 17,
            ["computers"]   = 18,
            ["tech"]        = 18,
            ["math"]        = 19,
            ["maths"]       = 19,
            ["mythology"]   = 20,
            ["sports"]      = 21,
            ["geography"]   = 22,
            ["geo"]         = 22,
            ["history"]     = 23,
            ["politics"]    = 24,
            ["art"]         = 25,
            ["celebrities"] = 26,
            ["animals"]     = 27,
            ["vehicles"]    = 28,
            ["cars"]        = 28,
            ["comics"]      = 29,
            ["anime"]       = 31,
            ["manga"]       = 31,
            ["cartoons"]    = 32,
        };

        public static async Task ProcessGuessAsync(CommandContext ctx, Channel channel, string rawGuess)
        {
            long channelId = ctx.ChannelId;

            if (!_sessions.TryGetValue(channelId, out var session))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "There's no active trivia question in this channel.");
                return;
            }

            if (session.Guesses.ContainsKey(ctx.Message.AuthorUserId))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "You've already submitted an answer!");
                return;
            }

            if (string.IsNullOrWhiteSpace(rawGuess))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please provide a letter. `A, B, C, or D`.");
                return;
            }

            char given = char.ToUpper(rawGuess[0]);
            if (given < 'A' || given > 'D')
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Invalid choice. Please guess A, B, C, or D.");
                return;
            }

            session.Guesses[ctx.Message.AuthorUserId] = new GuessEntry(given, ctx.Member.Name ?? "Unknown");
            await MessageHelper.ReplyAsync(ctx, channel, "📬 Answer submitted!");
        }

        public async Task Execute(CommandContext ctx)
        {
            ConcurrentDictionary<long, Channel> channelCache = ctx.ChannelCache;
            long channelId = ctx.ChannelId;
            PlanetMember member = ctx.Member;
            string[] args = ctx.Args;

            if (!channelCache.TryGetValue(channelId, out var channel)) return;

            // sd/trivia topics
            if (args.Length >= 1 && (args[0].ToLower() == "topics" || args[0].ToLower() == "t"))
            {
                string topicList = string.Join(", ", Categories.Keys.Order());
                await MessageHelper.ReplyAsync(ctx, channel, $"**Available topics:** {topicList}\n**Difficulties:** easy, medium, hard");
                return;
            }

            // sd/trivia — fetch a new question
            if (_sessions.ContainsKey(channelId))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "There's already an active trivia question in this channel!");
                return;
            }

            string? difficulty = null;
            int? categoryId = null;

            foreach (string arg in args.Select(a => a.ToLower()))
            {
                if (Difficulties.Contains(arg))
                    difficulty = arg;
                else if (Categories.TryGetValue(arg, out int id))
                    categoryId = id;
            }

            // Pick a random category if none was specified
            if (categoryId is null)
            {
                var ids = Categories.Values.Distinct().ToArray();
                categoryId = ids[Random.Shared.Next(ids.Length)];
            }

            string url = "https://opentdb.com/api.php?amount=1&type=multiple";
            if (difficulty is not null) url += $"&difficulty={difficulty}";
            if (categoryId is not null) url += $"&category={categoryId}";

            string rawJson;
            try
            {
                rawJson = await _http.GetStringAsync(url);
            }
            catch
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Failed to fetch a trivia question. Try again in a moment.");
                return;
            }

            using var doc = JsonDocument.Parse(rawJson);
            var result = doc.RootElement.GetProperty("results")[0];

            string question          = WebUtility.HtmlDecode(result.GetProperty("question").GetString()!);
            string correct           = WebUtility.HtmlDecode(result.GetProperty("correct_answer").GetString()!);
            string category          = WebUtility.HtmlDecode(result.GetProperty("category").GetString()!);
            string fetchedDifficulty = result.GetProperty("difficulty").GetString() ?? "unknown";

            List<string> answers = result.GetProperty("incorrect_answers").EnumerateArray()
                .Select(x => WebUtility.HtmlDecode(x.GetString()!))
                .Append(correct)
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            char correctLetter = (char)('A' + answers.IndexOf(correct));

            string questionText = string.Join("\n",
                $"**Category**: {category} | **Difficulty**: {fetchedDifficulty.ToTitleCase()}",
                "",
                $"**{question}**",
                "",
                string.Join("\n", answers.Select((a, i) => $"{(char)('A' + i)}) {a}")),
                "",
                $"*Use `{Config.Prefix}tg <A/B/C/D>` — you have 30 seconds!*"
            );

            var sent = await MessageHelper.ReplyAsync(ctx, channel, questionText);
            if (!sent.Success || sent.Data is null) return;

            Message botMessage = ctx.Client.Cache.Messages.TryGet(sent.Data.Id, out var cachedSent) && cachedSent is not null
                ? cachedSent : sent.Data;

            var newSession = new TriviaSession(correctLetter, questionText, answers, botMessage, channel, ctx.Client, new());
            _sessions[channelId] = newSession;

            _ = Task.Run(async () =>
            {
                await Task.Delay(30_000);
                _sessions.TryRemove(channelId, out _);

                List<string> corrects = [..newSession.Guesses
                    .Where(kv => kv.Value.Letter == newSession.CorrectLetter)
                    .Select(kv => kv.Value.MemberName)];

                List<string> wrongs = [..newSession.Guesses
                    .Where(kv => kv.Value.Letter != newSession.CorrectLetter)
                    .Select(kv => kv.Value.MemberName)];

                string correctAnswer = newSession.Answers[newSession.CorrectLetter - 'A'];
                string resultsText = $"⏰ **Time's up!** The answer was **{newSession.CorrectLetter}) {correctAnswer}**\n";

                if (corrects.Count > 0)
                    resultsText += $"\n✅ **Correct:** {string.Join(", ", corrects)}";
                if (wrongs.Count > 0)
                    resultsText += $"\n❌ **Wrong:** {string.Join(", ", wrongs)}";
                if (newSession.Guesses.IsEmpty)
                    resultsText += "\n😶 Nobody answered!";

                var resultMsg = new Message(newSession.Client)
                {
                    Content = resultsText,
                    ChannelId = newSession.Channel.Id,
                    PlanetId = newSession.Channel.Planet!.Id,
                    AuthorUserId = newSession.Client.Me.Id,
                    AuthorMemberId = newSession.Channel.Planet?.MyMember.Id,
                    ReplyToId = newSession.BotMessage.Id,
                    Fingerprint = Guid.NewGuid().ToString()
                };

                await newSession.Client.MessageService.SendMessage(resultMsg);
            });
        }
    }

    public class TriviaGuess : ICommand
    {
        public string Name => "tg";
        public string[] Aliases => [];
        public string Description => "Submit your answer to the active Trivia question.";
        public string Section => "Fun";
        public string Usage => "tg <A/B/C/D>";

        public async Task Execute(CommandContext ctx)
        {
            if (!ctx.ChannelCache.TryGetValue(ctx.ChannelId, out var channel)) return;

            if (ctx.Args.Length == 0 || string.IsNullOrWhiteSpace(ctx.Args[0]))
            {
                await MessageHelper.ReplyAsync(ctx, channel, "Please provide a letter. `A, B, C, or D`.");
                return;
            }

            await Trivia.ProcessGuessAsync(ctx, channel, ctx.Args[0]);
        }
    }
}
