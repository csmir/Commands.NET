namespace Commands.Testing;

/// <summary>
///     A caller context for testing purposes. When responding, the message is ignored.
/// </summary>
public class TestCallerContext : ICallerContext
{
    /// <inheritdoc />
    public void Respond(object? message) { }
}
