namespace Commands.Testing;

/// <summary>
///     A result type which contains the result of a test execution.
/// </summary>
public readonly struct TestResult : IExecuteResult
{
    /// <summary>
    ///     The command that was tested.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    ///     The actual result of the test. If the test succeeded, this will be the same as <see cref="ExpectedResult"/>.
    /// </summary>
    public TestResultType ActualResult { get; }

    /// <summary>
    ///     The expected result of the test.
    /// </summary>
    public TestResultType ExpectedResult { get; }

    /// <inheritdoc />
    public Exception? Exception { get; }

    /// <inheritdoc />
    public bool Success { get; }

    private TestResult(Command command, TestResultType expectedResult, TestResultType actualResult, Exception? exception, bool success)
    {
        Command = command;
        ExpectedResult = expectedResult;
        ActualResult = actualResult;
        Exception = exception;
        Success = success;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"Command = {Command} \nSuccess = {(Exception == null ? "True" : $"False \nException = {Exception.Message}")}";

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
    /// <param name="command">The command that was tested.</param>
    /// <param name="resultType">The result type of the test execution.</param>
    /// <returns></returns>
    public static TestResult FromSuccess(Command command, TestResultType resultType)
        => new(command, resultType, resultType, null, true);

    /// <summary>
    ///     Creates a new <see cref="TestResult"/> representing a failed test execution.
    /// </summary>
    /// <param name="command">The command that was tested.</param>
    /// <param name="expectedResult">The expected result of the test.</param>
    /// <param name="actualResult">The actual result of the test execution.</param>
    /// <param name="exception">An exception that might have occurred during test execution.</param>
    /// <returns></returns>
    public static TestResult FromError(Command command, TestResultType expectedResult, TestResultType actualResult, Exception exception)
        => new(command, expectedResult, actualResult, exception, false);
}
