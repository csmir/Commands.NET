using Commands.Core;

namespace Commands.Tests
{
    public sealed class Module : ModuleBase
    {
        [Command("test")]
        public void Test()
        {
            Console.WriteLine("Tested");
        }
    }
}
