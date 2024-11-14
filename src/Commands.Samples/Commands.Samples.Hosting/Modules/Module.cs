namespace Commands.Samples
{
    public sealed class Module : ModuleBase<ConsumerBase>
    {
        [Name("help")]
        public IEnumerable<string> Help()
            => Manager.GetCommands().Select(x => x.ToString());
    }
}
