using Commands.Core;

namespace Commands.Samples.Hosting.Modules
{
    public sealed class Module : ModuleBase<ConsumerBase>
    {
        [Name("help")]
        public void Help()
            => Console.WriteLine("Helped");
    }
}
