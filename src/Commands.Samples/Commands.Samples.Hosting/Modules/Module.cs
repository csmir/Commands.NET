using Commands.Core;

namespace Commands.Samples.Hosting.Modules
{
    public sealed class Module : ModuleBase<ConsumerBase>
    {
        [Command("help")]
        public void Help()
            => Console.WriteLine("Helped");
    }
}
