namespace Commands.Tests
{
    public sealed class Module : ModuleBase<ConsumerBase>
    {
        [Name("base-test")]
        public void Test()
        {
            
        }

        [Name("param-test")]
        public void Test(int i)
        {
            if (i == 0)
                return;
        }

        [Name("nested")]
        public sealed class NestedModule(IServiceProvider services) : ModuleBase<ConsumerBase>
        {
            private readonly IServiceProvider _services = services;

            [Name("test")]
            public void Test()
            {
                Console.WriteLine(_services);
            }
        }
    }
}
