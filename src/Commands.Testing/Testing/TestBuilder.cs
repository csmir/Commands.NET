namespace Commands.Testing;

/// <summary>
///     A set of properties for a test provider.
/// </summary>
public sealed class TestBuilder
{
    private string? _arguments;
    private TestResultType _result;

    /// <summary>
    ///     Creates a new instance of <see cref="TestBuilder"/>.
    /// </summary>
    public TestBuilder()
    {
        _result = TestResultType.Success;
    }

    /// <summary>
    ///     Sets the arguments this provider should test with.
    /// </summary>
    /// <param name="arguments">The arguments to set.</param>
    /// <returns>The same <see cref="TestBuilder"/> for call-chaining.</returns>
    public TestBuilder AddArguments(string? arguments)
    {
        _arguments = arguments;

        return this;
    }

    /// <summary>
    ///     Sets the result this provider should return.
    /// </summary>
    /// <param name="result">The result to set.</param>
    /// <returns>The same <see cref="TestBuilder"/> for call-chaining.</returns>
    public TestBuilder AddResult(TestResultType result)
    {
        _result = result;

        return this;
    }

    /// <summary>
    ///     Converts the properties to a new instance of <see cref="Test"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Test"/>.</returns>
    public Test Build()
    {
        return new Test(_arguments ?? string.Empty, _result);
    }
}
