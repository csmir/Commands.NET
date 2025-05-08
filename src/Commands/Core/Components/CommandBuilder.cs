using Commands.Conditions;

namespace Commands;

/// <summary>
///     A set of properties of a command.
/// </summary>
public sealed class CommandBuilder : IComponentBuilder
{
    private readonly List<IExecuteConditionBuilder> _conditions;
    private readonly List<string> _names;

    private Delegate? _delegate;

    /// <summary>
    ///     Creates a new instance of <see cref="CommandBuilder"/>.
    /// </summary>
    public CommandBuilder()
    {
        _conditions = [];
        _names = [];

        _delegate = null;
    }

    /// <summary>
    ///     Adds a name to the command group.
    /// </summary>
    /// <remarks>
    ///     This add-operation is case-insensitive.
    /// </remarks>
    /// <param name="name">The value to add. If this value already exists in the properties, it is ignored.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddName(string name)
    {
        Assert.NotNullOrEmpty(name, nameof(name));

        if (!_names.Contains(name))
            _names.Add(name);

        return this;
    }

    /// <summary>
    ///     Adds multiple names to the command group.
    /// </summary>
    /// <remarks>
    ///     This add-operation is case-insensitive.
    /// </remarks>
    /// <param name="names">The values to add. If any value already exists in the properties, it is ignored.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddNames(params string[] names)
    {
        foreach (var name in names)
            AddName(name);

        return this;
    }

    /// <summary>
    ///     Adds a condition to the command group.
    /// </summary>
    /// <param name="condition">The condition to add to the group.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddCondition(IExecuteConditionBuilder condition)
    {
        Assert.NotNull(condition, nameof(condition));

        _conditions.Add(condition);

        return this;
    }

    /// <summary>
    ///     Adds a condition to the command group.
    /// </summary>
    /// <param name="condition">The condition to add to the group.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddCondition(ExecuteCondition condition)
        => AddCondition(new ExecuteConditionBuilder(condition));

    /// <summary>
    ///     Adds multiple conditions to the command group.
    /// </summary>
    /// <param name="conditions">The conditions to add to the group.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddConditions(params IExecuteConditionBuilder[] conditions)
    {
        foreach (var condition in conditions)
            AddCondition(condition);

        return this;
    }

    /// <summary>
    ///     Adds multiple conditions to the command group.
    /// </summary>
    /// <param name="conditions">The conditions to add to the group.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddConditions(params ExecuteCondition[] conditions)
    {
        foreach (var condition in conditions)
            AddCondition(new ExecuteConditionBuilder(condition));

        return this;
    }

    /// <summary>
    ///     Defines a delegate to execute when the command is invoked.
    /// </summary>
    /// <remarks>
    ///     Delegate commands support accessing execution context by implementing <see cref="CommandContext{T}"/> as the first parameter of the delegate.
    /// </remarks>
    /// <param name="executionDelegate">The delegate to be considered the command body.</param>
    /// <returns>The same <see cref="CommandBuilder"/> for call-chaining.</returns>
    public CommandBuilder AddDelegate(Delegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    /// <summary>
    ///     Converts the properties into a new instance of <see cref="Command"/> which implements all configured values.
    /// </summary>
    /// <param name="configuration">The configuration object to configure this object during creation.</param>
    /// <returns>A new instance of <see cref="Command"/>.</returns>
    public IComponent Build(ComponentConfiguration? configuration = null)
    {
        var conditionsToAdd = _conditions.Select(condition => condition.Build());

        return new Command(_delegate!, conditionsToAdd, [.. _names], configuration);
    }
}
