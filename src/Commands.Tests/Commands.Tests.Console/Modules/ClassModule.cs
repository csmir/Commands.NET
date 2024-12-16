namespace Commands.Tests
{
    [Name("class-based", "cb")]
    [ANDCondition(true)]
    [ORCondition(true)]
    public class ClassModule : CommandModule
    {
        public void Run()
        {
            Respond(Command.Attributes.Length);
            Respond("Succesfully ran " + Command.ToString());
        }
    }
}
