<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
</head>
<body>
<h1>Privacy Policy</h1>
<p><strong>Effective Date:</strong> March 20, 2026</p>
<p>This Privacy Policy describes how SkyBot ("the Bot") collects, uses, and stores information when used within a Valour planet.</p>
<hr>
<h2>1. Information Collected</h2>
<p>The Bot collects only the minimum data necessary to provide its intended functionality. Most data is stored in-memory and is lost when the Bot restarts. A small amount of server configuration data is persisted to a local SQLite database for features that require it.</p>
<h3>Information Temporarily Held in Memory:</h3>
<ol>
  <li>Channel IDs (for routing messages and commands)</li>
  <li>Planet IDs (for planet-specific operations)</li>
  <li>Member IDs (for moderation commands and game session tracking)</li>
  <li>Member display names (for game contributor lists in Hangman, Wordle, and Trivia)</li>
</ol>
<h3>Information Persisted to Disk:</h3>
<p>The following server configuration data is saved to a local SQLite database so that it survives restarts:</p>
<ol>
  <li>Planet IDs (to associate configuration with a planet)</li>
  <li>Channel IDs (to remember the configured welcome channel)</li>
  <li>Welcome message template (the text set by a moderator via <code>setwelcome message</code>)</li>
  <li>Welcome system active state (enabled/disabled)</li>
</ol>
<p>This data contains no personal user information. It is server configuration set by planet moderators and is stored locally on the host running the Bot.</p>
<h3>Information Never Stored:</h3>
<ol>
  <li>Message content</li>
  <li>Direct Messages ("DMs")</li>
  <li>Personal account information (including usernames, email addresses, or other personally identifiable information)</li>
</ol>
<hr>
<h2>2. Purpose of Data Collection</h2>
<p>Temporarily held information is used exclusively to:</p>
<ol>
  <li>Route commands to the correct channels and planets</li>
  <li>Enable moderation commands such as ban, unban, and kick</li>
  <li>Track active game sessions (Hangman, Wordle, Trivia) and display contributor lists</li>
  <li>Enable core bot functionality during the current session</li>
</ol>
<p>The Bot does not use any information for profiling, marketing, analytics, or tracking purposes.</p>
<hr>
<h2>3. Data Storage and Security</h2>
<p>Most data is stored in-memory only and is automatically cleared when the Bot restarts. The exception is server configuration for the welcome system (see Section 1), which is written to a local SQLite database on the host machine. No data is sent to any external storage or cloud service.</p>
<p>The Bot does not sell, rent, trade, or otherwise share any data with third parties.</p>
<p>Some features make outbound requests to third-party APIs to fetch content. These requests do not include any user data:</p>
<ul>
  <li><strong>Datamuse</strong> (datamuse.com) — word lists for Hangman and Wordle</li>
  <li><strong>Open Trivia Database</strong> (opentdb.com) — trivia questions for Trivia</li>
  <li><strong>The Cat API</strong> (thecatapi.com) — random cat images for the cat command</li>
  <li><strong>nekos.best</strong> (nekos.best) — hug GIFs for the hug command</li>
</ul>
<hr>
<h2>4. Data Retention</h2>
<p>In-memory data (game sessions, moderation context, etc.) is held only for the duration of the Bot's current session and is cleared on restart. Server configuration data for the welcome system is retained in a local SQLite database until explicitly changed or deleted by a planet moderator.</p>
<hr>
<h2>5. Self-Hosting</h2>
<p>SkyBot is designed for self-hosting. If you choose to host your own instance of SkyBot, you are responsible for the privacy and security of any data processed by your instance. This policy applies to the official instance of SkyBot only.</p>
<hr>
<h2>6. Future Changes to Logging or Data Practices</h2>
<p>If additional operational logging or data collection practices are introduced in the future, this Privacy Policy will be updated to reflect those changes prior to implementation.</p>
<p>Continued use of the Bot after updates to this policy constitutes acceptance of the revised policy.</p>
<hr>
<h2>7. Contact Information</h2>
<p>For privacy-related inquiries, requests, or concerns, please contact:</p>
<p><strong>Email:</strong> contact@skyjoshua.xyz</p>
</body>
</html>