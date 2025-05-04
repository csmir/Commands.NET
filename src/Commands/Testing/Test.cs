namespace Commands.Testing;

/// <inheritdoc cref="ITest" />
public sealed class Test : ITest
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; }

    /// <inheritdoc />
    public string Arguments { get; }

    internal Test(string arguments, TestResultType shouldEvaluateTo)
    {
        Arguments = arguments;
        ShouldEvaluateTo = shouldEvaluateTo;
    }

    #region Initializers

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="Test"/>.
    /// </summary>
    /// <param name="arguments">The arguments to test with.</param>
    /// <param name="testResult">The result to test for.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static TestProperties From(string? arguments = null, TestResultType testResult = TestResultType.Success)
        => new TestProperties().AddArguments(arguments).AddResult(testResult);

    #endregion
}