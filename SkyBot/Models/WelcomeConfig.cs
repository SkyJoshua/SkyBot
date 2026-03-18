public class WelcomeConfig
{
    public long PlanetId { get; set; }
    public long ChannelId { get; set; }
    public string Message { get; set; } = "Welcome to the planet, {username}!";
    public bool Active { get; set; } = false;
}