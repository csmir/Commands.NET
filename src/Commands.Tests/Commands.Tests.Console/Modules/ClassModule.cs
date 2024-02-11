using Commands.Core;

namespace Commands.Tests
{
    [Name("class-based", "cb")]
    public class ClassModule : ModuleBase
    {
        public static void Run(CommandContext context)
        {
            Console.WriteLine("Succesfully ran " + context.Command.ToString());
        }
    }
}
