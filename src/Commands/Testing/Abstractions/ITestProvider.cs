namespace Commands.Testing;

/// <summary>
///     Represents a test provider that can be used to test a command.
/// </summary>
public interface ITestProvider
{
    /// <summary>
    ///     Gets or sets the result that the test should return. If the test does not return this result, it will be considered a failure.
    /// </summary>
    public TestResultType ExpectedResult { get; set; }

    /// <summary>
    ///     Gets or sets the arguments that the test should use when invoking the command.
    /// </summary>
    /// <remarks>
    ///     If this property is set to an empty array, the test will not provide any arguments to the command.
    /// </remarks>
    public KeyValuePair<string, object?>[] Arguments { get; set; }

    /// <summary>
    ///     Runs the test asynchronously from the configured values, with the provided options.
    /// </summary>
    /// <param name="caller">The caller context that is used to run the test.</param>
    /// <param name="command">The command that is being tested.</param>
    /// <param name="options">The options that determine how this command execution is approached. Some values from these options might not be evaluated for this test.</param>
    /// <returns>An awaitable <see cref="ValueTask{TResult}"/> containing the result of this test.</returns>
    public ValueTask<TestResult> Run<T>(T caller, Command command, CommandOptions? options = null)
        where T : ICallerContext;
}
