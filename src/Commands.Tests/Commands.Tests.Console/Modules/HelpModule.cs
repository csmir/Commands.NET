using Commands.Core;
using Commands.Reflection;

namespace Commands.Tests
{
    public sealed class HelpModule : ModuleBase
    {
        private readonly CommandManager _manager;

        public HelpModule(CommandManager manager)
        {
            _manager = manager;
        }

        [Command("help")]
        public void Help()
        {
            foreach (var command in _manager.Commands)
            {
                if (command is ModuleInfo module)
                {
                    PrintRecursive(module);
                }
                else
                {
                    Console.WriteLine(command);
                }
            }
        }

        private void PrintRecursive(ModuleInfo module, int stepHeight = 0)
        {
            foreach (var component in module.Components)
            {
                if (component is ModuleInfo moduleInfo)
                {
                    PrintRecursive(moduleInfo, stepHeight + 1);
                }
                else if (component is CommandInfo commandInfo)
                {
                    Console.WriteLine(commandInfo.ToString());
                }
            }
        }
    }
}
