namespace Commands.Tests
{
    [Name("class-based", "cb")]
    [ANDCondition(true)]
    [ORCondition(true)]
    public class ClassModule : ModuleBase
    {
        public void Run()
        {
            Send(Command.Attributes.Length);
            Send("Succesfully ran " + Command.ToString());
        }
    }
}
