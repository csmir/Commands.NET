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
        public sealed class NestedModule : ModuleBase<ConsumerBase>
        {
            [Name("test")]
            public void Test()
            {

            }
        }
    }
}
