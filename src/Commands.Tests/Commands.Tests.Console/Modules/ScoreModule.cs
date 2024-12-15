namespace Commands.Tests
{
    [Name("score")]
    public class ScoreModule : CommandModule
    {
        [Name("command")]
        public void ByBool(bool b)
        {
            Send("ByBool " + b);
        }

        [Name("command")]
        public void ByInt(int i)
        {
            Send("ByInt " + i);
        }

        public void Default()
        {
            Send("This is a default overload");
        }

        public void Default([Remainder] string? args = null)
        {
            Send($"This is a default overload with args: {args}");
        }
    }
}
