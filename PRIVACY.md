# Privacy Policy

**Effective Date:** March 20, 2026

This Privacy Policy describes how SkyBot ("the Bot") collects, uses, and stores information when used within a Valour planet.

---

## 1. Information Collected

The Bot collects only the minimum data necessary to provide its intended functionality. Most data is stored in-memory and is lost when the Bot restarts. A small amount of server configuration data is persisted to a local SQLite database for features that require it.

### Information Temporarily Held in Memory

1. Channel IDs (for routing messages and commands)
2. Planet IDs (for planet-specific operations)
3. Member IDs (for moderation commands and game session tracking)
4. Member display names (for game contributor lists in Hangman, Wordle, and Trivia)

### Information Persisted to Disk

The following server configuration data is saved to a local SQLite database so that it survives restarts:

1. Planet IDs (to associate configuration with a planet)
2. Channel IDs (to remember the configured welcome channel)
3. Welcome message template (the text set by a moderator via `setwelcome message`)
4. Welcome system active state (enabled/disabled)

This data contains no personal user information. It is server configuration set by planet moderators and is stored locally on the host running the Bot.

### Information Never Stored

1. Message content
2. Direct Messages ("DMs")
3. Personal account information (including usernames, email addresses, or other personally identifiable information)

---

## 2. Purpose of Data Collection

Temporarily held information is used exclusively to:

1. Route commands to the correct channels and planets
2. Enable moderation commands such as ban, unban, and kick
3. Track active game sessions (Hangman, Wordle, Trivia) and display contributor lists
4. Enable core bot functionality during the current session

The Bot does not use any information for profiling, marketing, analytics, or tracking purposes.

---

## 3. Data Storage and Security

Most data is stored in-memory only and is automatically cleared when the Bot restarts. The exception is server configuration for the welcome system (see Section 1), which is written to a local SQLite database on the host machine. No data is sent to any external storage or cloud service.

The Bot does not sell, rent, trade, or otherwise share any data with third parties.

Some features make outbound requests to third-party APIs to fetch content. These requests do not include any user data:

- **Datamuse** (datamuse.com) — word lists for Hangman and Wordle
- **Open Trivia Database** (opentdb.com) — trivia questions for Trivia
- **The Cat API** (thecatapi.com) — random cat images for the cat command
- **nekos.best** (nekos.best) — hug GIFs for the hug command
- **Pixabay** (pixabay.com) — images for the image command

---

## 4. Data Retention

In-memory data (game sessions, moderation context, etc.) is held only for the duration of the Bot's current session and is cleared on restart. Server configuration data for the welcome system is retained in a local SQLite database until explicitly changed or deleted by a planet moderator.

---

## 5. Self-Hosting

SkyBot is designed for self-hosting. If you choose to host your own instance of SkyBot, you are responsible for the privacy and security of any data processed by your instance. This policy applies to the official instance of SkyBot only.

---

## 6. Future Changes to Logging or Data Practices

If additional operational logging or data collection practices are introduced in the future, this Privacy Policy will be updated to reflect those changes prior to implementation.

Continued use of the Bot after updates to this policy constitutes acceptance of the revised policy.

---

## 7. Contact Information

For privacy-related inquiries, requests, or concerns, please contact:

**Email:** contact@skyjoshua.xyz