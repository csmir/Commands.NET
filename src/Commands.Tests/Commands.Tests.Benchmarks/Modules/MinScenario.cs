namespace Commands.Tests
{
    public sealed class MinScenario : CommandModule<CallerContext>
    {
        [Name("scenario")]
        public void Test()
        {

        }

        [Name("scenario-parameterized")]
        public void Test(int i)
        {
            if (i == 0)
                return;
        }

        [Name("scenario-nested")]
        public sealed class NestedModule(IServiceProvider services) : CommandModule<CallerContext>
        {
            private readonly IServiceProvider _services = services;

            [Name("scenario-injected")]
            public void Test()
            {
                Console.WriteLine(_services);
            }
        }
    }
}
