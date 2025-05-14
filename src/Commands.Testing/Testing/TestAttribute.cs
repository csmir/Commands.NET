namespace Commands.Testing;

/// <summary>
///     An attribute that is used to define a test for a command.
/// </summary>
/// <remarks>
///     To define tests for a command irrespective of its structural definition, create new instances of <see cref="Test"/> and provide them to the command using the <see cref="ITestContext"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TestAttribute : Attribute, ITest
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; set; } = TestResultType.Success;

    /// <inheritdoc />
    public string Arguments { get; set; } = string.Empty;
}
