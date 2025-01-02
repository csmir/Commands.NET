using Commands.Conditions;

namespace Commands.Builders;

/// <summary>
///     A builder model for an execution condition, which is an evaluation that is triggered based on the configured trigger moment, setting up the command to fail or succeed.
/// </summary>
public interface IConditionBuilder
{
    /// <summary>
    ///     Gets or sets the trigger that determines when the condition should be evaluated during the execution process.
    /// </summary>
    public ConditionTrigger Triggers { get; set; }

    /// <summary>
    ///     Builds an execution condition from the provided configuration.
    /// </summary>
    /// <returns>A new implementation of <see cref="ICondition"/> representing an evaluation for command execution to succeed or fail.</returns>
    public ICondition Build();
}
