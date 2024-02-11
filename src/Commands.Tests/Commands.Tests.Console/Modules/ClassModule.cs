using Commands.Core;

namespace Commands.Tests
{
    [Name("class-based", "cb")]
    public class ClassModule : ModuleBase
    {
        public void Run()
        {
            Console.WriteLine("Succesfully ran " + Command.ToString());
        }
    }
}
