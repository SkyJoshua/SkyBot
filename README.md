# SkyBot

SkyBot is a Valour.gg bot built with .NET 10.

---

## Features

- Designed for self-hosting
- Open-source under AGPL-3.0
- Built with .NET 10
- Command system with automatic registration

### Fun

- `8ball` — ask the magic 8 ball a question
- `coinflip` — flip a coin
- `dice` — roll a die
- `rockpaperscissors` — play rock paper scissors against the bot
- `choose` — pick one of the given options
- `echo` — repeat text through the bot
- `reverse` — reverse yours or a replied message
- `mock` — mOcK tExT
- `t9encode` / `t9decode` — encode or decode old phone keypad multi-tap digits
- `hangman` — channel-wide game of hangman with optional category (`hg <letter or word>` to guess)
- `wordle` — channel-wide Wordle; guess the 5-letter word in 6 tries (`wg <word>` to guess)
- `trivia` — channel-wide trivia question with 30 seconds to answer (`tg <A/B/C/D>` to guess)
- `image` — fetch a random image matching your search (aliases: `img`)

### Chill

- `cat` — post a random cat picture
- `hug` — send a hug with a random gif

### Info

- `ping` — check bot latency
- `uptime` — show how long the bot has been running
- `info` — user and planet info
- `version` — show the current bot and Valour SDK version
- `usercount` — show the total Valour user count
- `source` — link to the bot's source code
- `joinsite` — link to a site to help bots join a planet
- `devcentral` — invite link to the Dev Central planet
- `swagger` — link to the Valour API docs
- `minecraft` — Unofficial ValourSMP server IPs
- `suggest` — submit a suggestion for the bot

### Moderation

- `ban` / `unban` / `kick` — member moderation
- `bans` — list all bans in the planet
- `setwelcome` — configure a welcome channel and message

---

## Data & Privacy

SkyBot stores only the minimum data required for operation. Most data is stored in-memory and is lost on restart. A small amount of server configuration data is persisted to a local SQLite database for the welcome system.

SkyBot does **not** store:

- Message content
- Direct messages
- Personal user data

Full privacy policy:
https://git.skyjoshua.xyz/SkyJoshua/SkyBot/blob/main/PRIVACY.md

---

## License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

See the LICENSE file for details:
https://git.skyjoshua.xyz/SkyJoshua/SkyBot/blob/main/LICENSE

Because this project is licensed under AGPL-3.0, if you modify and deploy it publicly (including as a hosted service), you must make your source code available under the same license.

---

## Requirements

- .NET 10
- A Valour bot token
- A [Pixabay API key](https://pixabay.com/api/docs/) (free) — required for the `image` command

---

## Installation

```bash
git clone https://git.skyjoshua.xyz/SkyJoshua/SkyBot.git
cd SkyBot/SkyBot
dotnet restore
```

All required NuGet packages will be installed automatically using the provided `SkyBot.csproj` file.

---

## Configuration

Create a `.env` file in the root directory of the project:
```
TOKEN=your-bot-token-here
PIXABAY_API_KEY=your-pixabay-api-key-here
```

Then open `Config.cs` and update the following values:
```cs
public static readonly long OwnerId = your-owner-id-here;
public static readonly string Prefix = "your-prefix-here";
public static readonly string SourceLink = "your-source-link-here";
```

- Replace `your-owner-id-here` with your Valour user ID.
- Replace `your-prefix-here` with your desired command prefix (e.g. `s/`).
- Replace `your-source-link-here` with a link to your fork of the repository.

Never commit your `.env` file to the repository. Ensure it is listed in your `.gitignore`.

---

## Running the Bot
```bash
dotnet run
```