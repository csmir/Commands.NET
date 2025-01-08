namespace Commands.Testing;

/// <summary>
///     A result type which contains the result of a test execution.
/// </summary>
public readonly struct TestResult : IExecuteResult
{
    /// <summary>
    ///     The actual result of the test. If the test succeeded, this will be the same as <see cref="ITestProvider.ExpectedResult"/>.
    /// </summary>
    /// <remarks>
    ///     This value is <see langword="null"/> if the test succeeded as a whole.
    /// </remarks>
    public TestResultType? ActualResult { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success { get; }

    private TestResult(TestResultType? actualResult, Exception? exception, bool success)
    {
        ActualResult = actualResult;
        Exception = exception;
        Success = success;
    }

    /// <summary>
    ///     Creates a new <see cref="TestResult"/> representing a successful test execution.
    /// </summary>
    /// <param name="resultType">The result type of the test execution.</param>
    /// <returns></returns>
    public static TestResult FromSuccess(TestResultType resultType)
        => new(resultType, null, true);

    /// <summary>
    ///     Creates a new <see cref="TestResult"/> representing a failed test execution.
    /// </summary>
    /// <param name="actualResultType">The result type of the test execution.</param>
    /// <param name="exception">An exception that might have occurred during test execution.</param>
    /// <returns></returns>
    public static TestResult FromError(TestResultType actualResultType, Exception? exception = null)
        => new(actualResultType, exception, false);
}
