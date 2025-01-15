namespace Commands.Testing;

public sealed class TestRunnerProperties
{
    public TestRunnerProperties()
    {

    }

    public TestRunnerProperties Provider(TestProvider provider)
    {

    }

    public TestRunnerProperties Providers(params TestProvider[] providers)
    {

    }

    public TestRunner ToRunner()
    {

    }

    public static implicit operator TestRunner(TestRunnerProperties properties)
        => properties.ToRunner();
}
