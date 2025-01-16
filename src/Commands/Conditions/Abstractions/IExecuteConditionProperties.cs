namespace Commands.Conditions;

/// <summary>
///     Represents a set of properties to create an <see cref="ExecuteCondition"/>.
/// </summary>
public interface IExecuteConditionProperties
{
    /// <summary>
    ///     Converts the properties to an instance of <see cref="ExecuteCondition"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ExecuteCondition"/>.</returns>
    public ExecuteCondition Create();
}