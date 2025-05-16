namespace Commands.Testing;

/// <summary>
///     An attribute that is used to define a test for a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TestAttribute : Attribute, ITest
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; set; } = TestResultType.Success;

    /// <inheritdoc />
    public string Arguments { get; set; } = string.Empty;
}
