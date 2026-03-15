using System.Collections.Concurrent;

namespace SkyBot.Models
{
    public interface ICommand
    {
        string Name { get; }
        string[] Aliases { get; }
        string Description { get; }
        string Section { get; }
        string Usage { get; }
        Task Execute(CommandContext ctx);
    }
}