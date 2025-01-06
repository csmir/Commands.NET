namespace Commands.Builders;

/// <summary>
///     A builder model for an execution condition, which is an evaluation that is triggered based on the configured trigger moment, setting up the command to fail or succeed.
/// </summary>
public interface IConditionBuilder
{
    /// <summary>
    ///     Builds an execution condition from the provided configuration.
    /// </summary>
    /// <returns>A new implementation of <see cref="ICondition"/> representing an evaluation for command execution to succeed or fail.</returns>
    public ICondition Build();
}
