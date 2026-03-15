using SkyBot.Models;

namespace SkyBot.Commands
{
    public static class CommandRegistry
    {
        public static readonly Dictionary<string, ICommand> Commands = new();
        public static readonly Dictionary<string, List<ICommand>> Sections = new();

        static CommandRegistry()
        {
            var allCommands = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (ICommand?)Activator.CreateInstance(t))
                .Select(c => c!);

            foreach (var cmd in allCommands)
            {
                Commands[cmd.Name.ToLower()] = cmd;
                foreach (var alias in cmd.Aliases)
                {
                    Commands[alias.ToLower()] = cmd;
                }

                Sections = Commands.Values
                    .Distinct()
                    .GroupBy(c => c.Section.ToLower())
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }
    }
}