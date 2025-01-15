using Commands.Conditions;

namespace Commands;

public sealed class CommandProperties : ComponentProperties
{
    private readonly List<ExecuteConditionProperties> _conditions;
    private readonly List<string> _names;

    private Delegate? _delegate;

    public CommandProperties()
    {
        _conditions = [];
        _names = [];

        _delegate = null;
    }

    public CommandProperties Name(string name)
    {
        Assert.NotNullOrEmpty(name, nameof(name));

        if (!_names.Contains(name))
            _names.Add(name);

        return this;
    }

    public CommandProperties Names(params string[] names)
    {
        foreach (var name in names)
            Name(name);

        return this;
    }

    public CommandProperties Condition(ExecuteConditionProperties condition)
    {
        Assert.NotNull(condition, nameof(condition));

        _conditions.Add(condition);

        return this;
    }

    public CommandProperties Conditions(params ExecuteConditionProperties[] conditions)
    {
        foreach (var condition in conditions)
            Condition(condition);

        return this;
    }

    public CommandProperties Handler(Delegate executionDelegate)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        _delegate = executionDelegate;

        return this;
    }

    public override IComponent ToComponent(CommandGroup? parent = null, ComponentConfiguration? configuration = null)
    {
        Assert.NotNull(_delegate, nameof(_delegate));

        configuration ??= ComponentConfiguration.Default;

        var conditionsToAdd = _conditions.Select(condition => condition.ToCondition());

        var delegateHasContext = _delegate!.Method.HasContextProvider();

        return new Command(parent, new CommandDelegateActivator(_delegate.Method, _delegate.Target, delegateHasContext), conditionsToAdd, [.. _names], delegateHasContext, configuration);
    }
}
