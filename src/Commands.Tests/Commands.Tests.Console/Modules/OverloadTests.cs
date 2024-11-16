namespace Commands.Tests
{
    [Name("overload")]
    public class OverloadTests : ModuleBase
    {
        public void Get(bool value = false)
        {

        }

        [Name("get")]
        public static void Get(string id = "")
        {

        }

        [Name("get-other")]
        public static void Get(string id, string name)
        {

        }
    }
}
