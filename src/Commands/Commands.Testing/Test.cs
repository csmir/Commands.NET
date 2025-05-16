namespace Commands.Testing;

/// <summary>
///     An implementation of <see cref="ITest"/> that can be used to define a test for a command irrespective of its structural definition.
/// </summary>
/// <remarks>
///     <see cref="TestAttribute"/> can be used to implement tests on command definitions.
/// </remarks>
public sealed class Test : ITest
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; }

    /// <inheritdoc />
    public string Arguments { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="Test"/> without any arguments, which should yield success to be considered a passing test.
    /// </summary>
    public Test()
        : this(string.Empty, TestResultType.Success) { }

    /// <summary>
    ///     Creates a new instance of <see cref="Test"/> without any arguments, expecting <paramref name="shouldEvaluateTo"/> as the test result.
    /// </summary>
    /// <param name="shouldEvaluateTo">The result that should be yielded by the tested command. If the result deviates from the expected result, the test will fail.</param>
    public Test(TestResultType shouldEvaluateTo)
        : this(string.Empty, shouldEvaluateTo) { }

    /// <summary>
    ///     Creates a new instance of <see cref="Test"/> using the provided arguments and expected result type.
    /// </summary>
    /// <param name="arguments">The arguments to provide to the command for execution under a test.</param>
    /// <param name="shouldEvaluateTo">The result that should be yielded by the tested command. If the result deviates from the expected result, the test will fail.</param>
    public Test(string arguments, TestResultType shouldEvaluateTo)
    {
        Arguments = arguments;
        ShouldEvaluateTo = shouldEvaluateTo;
    }
}