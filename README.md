<!DOCTYPE html>
<html lang="en">
<body>
<h1>SkyBot</h1>
<p>
SkyBot is a Valour.gg bot built with .NET 10.
</p>
<h2>Features</h2>
<ul>
    <li>Designed for self-hosting</li>
    <li>Open-source under AGPL-3.0</li>
    <li>Built with .NET 10</li>
    <li>Command system with automatic registration</li>
</ul>
<h3>Fun</h3>
<ul>
    <li>8ball — ask the magic 8 ball a question</li>
    <li>coinflip — flip a coin</li>
    <li>dice — roll a die</li>
    <li>rockpaperscissors — play rock paper scissors against the bot</li>
    <li>choose — pick one of the given options</li>
    <li>echo — repeat text through the bot</li>
    <li>reverse — reverse yours or a replied message</li>
    <li>mock — mOcK tExT</li>
    <li>t9encode / t9decode — encode or decode old phone keypad multi-tap digits</li>
    <li>hangman — channel-wide game of hangman with optional category (<code>hg &lt;letter or word&gt;</code> to guess)</li>
    <li>wordle — channel-wide Wordle; guess the 5-letter word in 6 tries (<code>wg &lt;word&gt;</code> to guess)</li>
    <li>trivia — channel-wide trivia question with 30 seconds to answer (<code>tg &lt;A/B/C/D&gt;</code> to guess)</li>
    <li>image — fetch a random image matching your search (aliases: <code>img</code>)</li>
</ul>
<h3>Chill</h3>
<ul>
    <li>cat — post a random cat picture</li>
    <li>hug — send a hug with a random gif</li>
</ul>
<h3>Info</h3>
<ul>
    <li>ping — check bot latency</li>
    <li>uptime — show how long the bot has been running</li>
    <li>info — user and planet info</li>
    <li>version — show the current bot and Valour SDK version</li>
    <li>usercount — show the total Valour user count</li>
    <li>source — link to the bot's source code</li>
    <li>joinsite — link to a site to help bots join a planet</li>
    <li>devcentral — invite link to the Dev Central planet</li>
    <li>swagger — link to the Valour API docs</li>
    <li>minecraft — Unofficial ValourSMP server IPs</li>
    <li>suggest — submit a suggestion for the bot</li>
</ul>
<h3>Moderation</h3>
<ul>
    <li>ban / unban / kick — member moderation</li>
    <li>bans — list all bans in the planet</li>
    <li>setwelcome — configure a welcome channel and message</li>
</ul>
<h2>Data &amp; Privacy</h2>
<p>SkyBot stores only the minimum data required for operation. All data is stored in-memory and is lost on restart. SkyBot does <strong>not</strong> persist any data to disk.</p>
<p>SkyBot does <strong>not</strong> store:</p>
<ul>
    <li>Message content</li>
    <li>Direct messages</li>
    <li>Personal user data</li>
</ul>
<p>
Full privacy policy:<br>
<a href="https://github.com/SkyJoshua/SkyBot/blob/main/PRIVACY.md">
https://github.com/SkyJoshua/SkyBot/blob/main/PRIVACY.md
</a>
</p>
<h2>License</h2>
<p>
This project is licensed under the
<strong>GNU Affero General Public License v3.0 (AGPL-3.0)</strong>.
</p>
<p>
See the LICENSE file for details:<br>
<a href="https://github.com/SkyJoshua/SkyBot/blob/main/LICENSE">
https://github.com/SkyJoshua/SkyBot/blob/main/LICENSE
</a>
</p>
<p>
Because this project is licensed under AGPL-3.0, if you modify and deploy it
publicly (including as a hosted service), you must make your source code
available under the same license.
</p>
<h2>Requirements</h2>
<ul>
    <li>.NET 10</li>
    <li>A Valour bot token</li>
    <li>A <a href="https://pixabay.com/api/docs/">Pixabay API key</a> (free) — required for the <code>image</code> command</li>
</ul>
<h2>Installation</h2>
<p>Fork this repository, then:</p>
<pre><code>git clone https://github.com/YOUR_USERNAME/SkyBot.git
cd SkyBot/SkyBot
dotnet restore
</code></pre>
<p>
All required NuGet packages will be installed automatically using the
provided <code>SkyBot.csproj</code> file.
</p>
<h2>Configuration</h2>
<p>Create a <code>.env</code> file in the root directory of the project with your bot token:</p>
<pre><code>TOKEN=your-bot-token-here
PIXABAY_API_KEY=your-pixabay-api-key-here
</code></pre>
<p>Then open <code>Config.cs</code> and update the following values:</p>
<pre><code>public static readonly long OwnerId = your-owner-id-here;
public static readonly string Prefix = "your-prefix-here";
public static readonly string SourceLink = "your-source-link-here";
</code></pre>
<ul>
    <li>Replace <code>your-owner-id-here</code> with your Valour user ID.</li>
    <li>Replace <code>your-prefix-here</code> with your desired command prefix (e.g. <code>s/</code>).</li>
    <li>Replace <code>your-source-link-here</code> with a link to your fork of the repository.</li>
</ul>
<p>Never commit your <code>.env</code> file to the repository. Ensure it is listed in your <code>.gitignore</code>.</p>
<h2>Running the Bot</h2>
<pre><code>dotnet run</code></pre>
<h2>Contributing</h2>
<p>
Contributions are welcome. By submitting a contribution, you agree that your
contributions will be licensed under AGPL-3.0.
</p>
<ol>
    <li>Fork the repository</li>
    <li>Create a feature branch</li>
    <li>Submit a pull request</li>
</ol>
</body>
</html>