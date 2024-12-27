namespace Commands.Tests
{
    [Name("class-based", "cb")]
    [ANDCondition(true)]
    [ORCondition(true)]
    public class ClassModule : CommandModule
    {
        public void Run()
        {
            var attribute = Command.GetAttribute<ANDConditionAttribute>();

            Respond(Command.Attributes.Length);
            Respond("Succesfully ran " + Command.ToString());
        }
    }
}
