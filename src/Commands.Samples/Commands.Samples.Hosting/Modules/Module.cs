namespace Commands.Samples
{
    public sealed class Module : ModuleBase<ConsumerBase>
    {
        [Name("help")]
        public void Help()
            => Console.WriteLine("Helped");
    }
}
