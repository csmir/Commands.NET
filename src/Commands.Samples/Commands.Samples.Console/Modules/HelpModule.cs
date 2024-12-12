using Commands.Reflection;
using System.ComponentModel;

namespace Commands.Samples
{
    public class HelpModule : ModuleBase
    {
        [Name("help")]
        public async Task Help()
        {
            var commands = Tree.GetCommands();

            foreach (var command in commands)
            {
                var description = command.GetAttribute<DescriptionAttribute>()?.Description ?? "No description available.";

                await Consumer.Send(command.ToString() ?? "Unnamed component.");
                await Consumer.Send(command.FullName ?? "Unnamed command.");
                await Consumer.Send(description);
            }
        }
    }
}
