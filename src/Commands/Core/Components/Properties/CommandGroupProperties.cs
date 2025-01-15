using Commands.Conditions;

namespace Commands;

public sealed class CommandGroupProperties : ComponentProperties
{
    private readonly List<ExecuteConditionProperties> _conditions;
    private readonly List<ComponentProperties> _components;
    private readonly List<string> _names;

    public CommandGroupProperties()
    {
        _components = [];
        _names = [];
    }

    public CommandGroupProperties Name(string name)
    {
        Assert.NotNullOrEmpty(name, nameof(name));

        if (!_names.Contains(name))
            _names.Add(name);

        return this;
    }

    public CommandGroupProperties Names(params string[] names)
    {
        foreach (var name in names)
            Name(name);

        return this;
    }

    public CommandGroupProperties Condition(ExecuteConditionProperties condition)
    {
        Assert.NotNull(condition, nameof(condition));

        _conditions.Add(condition);

        return this;
    }

    public CommandGroupProperties Conditions(params ExecuteConditionProperties[] conditions)
    {
        foreach (var condition in conditions)
            Condition(condition);

        return this;
    }

    public CommandGroupProperties Component(ComponentProperties component)
    {
        Assert.NotNull(component, nameof(component));

        _components.Add(component);

        return this;
    }

    public CommandGroupProperties Components(params ComponentProperties[] componentDefinitions)
    {
        foreach (var component in componentDefinitions)
            Component(component);

        return this;
    }

    public override IComponent ToComponent(CommandGroup? parent = null, ComponentConfiguration? configuration = null)
    {
        configuration ??= ComponentConfiguration.Default;

        var conditionsToAdd = _conditions.Select(condition => condition.ToCondition());

        var group = new CommandGroup(parent, conditionsToAdd, [.. _names], configuration);

        var itemsToAdd = _components.Select(component => component.ToComponent(group, configuration));

        group.AddRange([.. itemsToAdd]);

        return group;
    }
}
