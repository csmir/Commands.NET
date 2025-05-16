namespace Commands.Testing;

/// <summary>
///     Represents a test provider that can be used to test a command.
/// </summary>
/// <remarks>
///     Create new instances of <see cref="Test"/> or define <see cref="TestAttribute"/> on commands to define new tests.
/// </remarks>
public interface ITest
{
    /// <summary>
    ///     Gets or sets the result that the test should return. If the test does not return this result, it will be considered a failure.
    /// </summary>
    public TestResultType ShouldEvaluateTo { get; }

    /// <summary>
    ///     Gets or sets the arguments that the test should use when invoking the command.
    /// </summary>
    /// <remarks>
    ///     If this property is set to an empty array, the test will not provide any arguments to the command.
    /// </remarks>
    public string Arguments { get; }
}
