namespace Commands.Tests
{
    [Name("class-based", "cb")]
    public class ClassModule : ModuleBase
    {
        public void Run()
        {
            Send("Succesfully ran " + Command.ToString());
        }
    }
}
