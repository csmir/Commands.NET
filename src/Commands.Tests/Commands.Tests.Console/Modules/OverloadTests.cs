namespace Commands.Tests
{
    [Name("overload")]
    public class OverloadTests : CommandModule
    {
        public string GetDefault => Get("someId");

        [Name("get")]
        public static string Get(string id = "")
        {
            return "OK!" + id;
        }

        [Name("get-other")]
        public static string Get(string id, string name)
        {
            return "OK!" + id + name;
        }
    }
}
