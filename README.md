<!DOCTYPE html>
<html lang="en">
<body>

<h1>SkyBot</h1>

<p>
SkyBot is a Valour.gg bot.
</p>

<h2>Features</h2>
<ul>
    <li>Designed for self-hosting</li>
    <li>Open-source under AGPL-3.0</li>
    <li>Built with .NET</li>
</ul>

<h2>Data &amp; Privacy</h2>
<p>SkyBot stores only the minimum data required for operation:</p>
<ul>
    <li>Message IDs</li>
    <li>Server (Planet) IDs</li>
    <li>Channel IDs</li>
</ul>

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

<h2>Installation</h2>
Fork this Repository
<pre><code>git clone https://github.com/YOUR_USERNAME/SkyBot.git
cd SkyBot
dotnet restore
</code></pre>

<p>
All required NuGet packages will be installed automatically using the
provided <code>SkyBot.csproj</code> file.
</p>

<h2>Configuration</h2>
<p>Before running the bot, create a <code>.env</code> file in the root directory of the project with the following content:</p>

<pre><code>TOKEN=your-bot-token-here
</code></pre>

<ul>
    <li>Replace <code>your-bot-token-here</code> with your actual bot token.</li>
    <li>Ensure the bot has proper permissions in the target server.</li>
</ul>

<p>
Sensitive data such as bot tokens should never be committed to the repository.
Use environment variables or secure configuration methods.
</p>

<h2>Running the Bot</h2>

<pre><code>dotnet run
</code></pre>

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
