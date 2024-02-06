using Commands.Core;

namespace Commands.Samples.Hosting.Modules
{
    public sealed class Module : ModuleBase<CommandContext>
    {
        private readonly IServiceProvider _provider;

        public Module(IServiceProvider provider)
        {
            _provider = provider;
        }

        [Command("help")]
        public void Help()
            => Console.WriteLine("Helped");
    }
}
