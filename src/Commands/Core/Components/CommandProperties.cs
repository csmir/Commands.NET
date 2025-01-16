using Commands.Conditions;

namespace Commands;

/// <summary>
///     A set of properties of a command.
/// </summary>
public sealed class CommandProperties : IComponentProperties
{
    private readonly List<IExecuteConditionProperties> _conditions;
    private readonly List<string> _names;

    private Delegate? _delegate;

    /// <summary>
    ///     Creates a new instance of <see cref="CommandProperties"/>.
    /// </summary>
    public CommandProperties()
    {
        _conditions = [];
        _names      = [];

        _delegate = null;
    }

    /// <summary>
    ///     Adds a name to the command group.
    /// </summary>
    /// <remarks>
    ///     This add-operation is case-insensitive.
    /// </remarks>
    /// <param name="name">The value to add. If this value already exists in the properties, it is ignored.</param>
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Name(string name)
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
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Names(params string[] names)
    {
        foreach (var name in names)
            Name(name);

        return this;
    }

    /// <summary>
    ///     Adds a condition to the command group.
    /// </summary>
    /// <param name="condition">The condition to add to the group.</param>
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Condition(IExecuteConditionProperties condition)
    {
        Assert.NotNull(condition, nameof(condition));

        _conditions.Add(condition);

        return this;
    }

    /// <summary>
    ///     Adds a condition to the command group.
    /// </summary>
    /// <param name="condition">The condition to add to the group.</param>
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Condition(ExecuteCondition condition)
        => Condition(new ExecuteConditionProperties(condition));

    /// <summary>
    ///     Adds multiple conditions to the command group.
    /// </summary>
    /// <param name="conditions">The conditions to add to the group.</param>
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Conditions(params IExecuteConditionProperties[] conditions)
    {
        foreach (var condition in conditions)
            Condition(condition);

        return this;
    }

    /// <summary>
    ///     Adds multiple conditions to the command group.
    /// </summary>
    /// <param name="conditions">The conditions to add to the group.</param>
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Conditions(params ExecuteCondition[] conditions)
    {
        foreach (var condition in conditions)
            Condition(new ExecuteConditionProperties(condition));

        return this;
    }

    /// <summary>
    ///     Defines a delegate to execute when the command is invoked.
    /// </summary>
    /// <remarks>
    ///     Delegate commands support accessing execution context by implementing <see cref="CommandContext{T}"/> as the first parameter of the delegate.
    /// </remarks>
    /// <param name="executionDelegate">The delegate to be considered the command body.</param>
    /// <returns>The same <see cref="CommandProperties"/> for call-chaining.</returns>
    public CommandProperties Delegate(Delegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    /// <summary>
    ///     Converts the properties into a new instance of <see cref="Command"/> which implements all configured values.
    /// </summary>
    /// <param name="parent">The parent object of this group. If left as null, the group will not inherit any configured values of said parent, such as conditions.</param>
    /// <param name="configuration">The configuration object to configure this object during creation.</param>
    /// <returns>A new instance of <see cref="Command"/>.</returns>
    public IComponent Create(CommandGroup? parent = null, ComponentConfiguration? configuration = null)
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        configuration ??= ComponentConfiguration.Default;

        var conditionsToAdd = _conditions.Select(condition => condition.Create());

        var delegateHasContext = _delegate!.Method.HasContextProvider();

        return new Command(parent, new CommandDelegateActivator(_delegate.Method, _delegate.Target, delegateHasContext), conditionsToAdd, [.. _names], delegateHasContext, configuration);
    }
}
