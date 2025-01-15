namespace Commands.Testing;

public sealed class TestProviderProperties
{
    private string _arguments;
    private Command? _command;
    private TestResultType _result;

    public TestProviderProperties()
    {
        _arguments = string.Empty;
        _command = null;
        _result = TestResultType.Success;
    }

    public TestProviderProperties Command(Command command)
    {
        Assert.NotNull(command, nameof(command));

        _command = command;

        return this;
    }

    public TestProviderProperties Arguments(string arguments)
    {
        Assert.NotNullOrEmpty(arguments, nameof(arguments));

        _arguments = arguments;

        return this;
    }

    public TestProviderProperties Result(TestResultType result)
    {
        _result = result;

        return this;
    }

    public TestProvider ToTest()
    {
        Assert.NotNull(_command, nameof(_command));

        return new TestProvider(_command!, _arguments, _result);
    }
}
