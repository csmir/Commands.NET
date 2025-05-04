namespace Commands.Testing;

/// <summary>
///     A result type which contains the result of a test execution.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct TestResult : IResult
{
    /// <summary>
    ///     Gets the executed test.
    /// </summary>
    public ITest Test { get; }

    /// <summary>
    ///     Gets the actual result of the test. If the test succeeded, this will be the same as <see cref="ExpectedResult"/>.
    /// </summary>
    public TestResultType ActualResult { get; }

    /// <summary>
    ///     Gets the expected result of the test.
    /// </summary>
    public TestResultType ExpectedResult { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success { get; }

    private TestResult(ITest test, TestResultType expectedResult, TestResultType actualResult, Exception? exception, bool success)
    {
        Test = test;
        ExpectedResult = expectedResult;
        ActualResult = actualResult;
        Exception = exception;
        Success = success;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"Test = {Test} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

    /// <summary>
    ///     Gets a string representation of this result.
    /// </summary>
    /// <param name="inline">Sets whether the string representation should be inlined or not.</param>
    /// <returns>A string containing a formatted value of the result.</returns>
    public string ToString(bool inline)
        => inline ? $"Success = {(Exception == null ? "True" : $"False")}" : ToString();

    /// <summary>
    ///     Creates a new <see cref="TestResult"/> representing a successful test execution.
    /// </summary>
    /// <param name="test">The test that was tested.</param>
    /// <param name="resultType">The result type of the test execution.</param>
    /// <returns></returns>
    public static TestResult FromSuccess(ITest test, TestResultType resultType)
        => new(test, resultType, resultType, null, true);

    /// <summary>
    ///     Creates a new <see cref="TestResult"/> representing a failed test execution.
    /// </summary>
    /// <param name="test">The test that was tested.</param>
    /// <param name="expectedResult">The expected result of the test.</param>
    /// <param name="actualResult">The actual result of the test execution.</param>
    /// <param name="exception">An exception that might have occurred during test execution.</param>
    /// <returns></returns>
    public static TestResult FromError(ITest test, TestResultType expectedResult, TestResultType actualResult, Exception exception)
        => new(test, expectedResult, actualResult, exception, false);
}
