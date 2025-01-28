namespace Commands.Testing;

/// <summary>
///     A caller context for testing purposes. When responding, the message is ignored.
/// </summary>
/// <param name="input">The input to use for the context.</param>
public class TestContext(string? input) : ICallerContext
{
    /// <inheritdoc />
    public ArgumentDictionary Arguments { get; } = ArgumentDictionary.From(input);

    /// <inheritdoc />
    public void Respond(object? message) { } // Deliberately emptied.
}
