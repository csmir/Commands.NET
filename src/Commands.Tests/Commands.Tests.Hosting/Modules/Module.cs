using Commands.Core;

namespace Commands.Tests
{
    public sealed class Module(IServiceProvider provider) : ModuleBase
    {
        private readonly IServiceProvider _provider = provider;

        [Command("test")]
        public void Test()
        {
            Console.WriteLine("Tested");
        }
    }
}
