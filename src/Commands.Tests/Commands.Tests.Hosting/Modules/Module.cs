using Commands.Core;

namespace Commands.Tests
{
    public sealed class Module : ModuleBase
    {
        [Name("test")]
        public void Test()
        {
            Console.WriteLine("Tested");
        }
    }
}
