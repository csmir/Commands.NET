using Commands.Conditions;

namespace Commands;

/// <summary>
///     Reveals information about a command.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class CommandInfo : IComponent, IArgumentBucket
{
    /// <inheritdoc />
    public IActivator Activator { get; }

    /// <inheritdoc />
    public string[] Aliases { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public ConditionEvaluator[] Evaluators { get; }

    /// <inheritdoc />
    public IArgument[] Arguments { get; }

    /// <inheritdoc />
    public bool HasRemainder { get; }

    /// <inheritdoc />
    public int MinLength { get; }

    /// <inheritdoc />
    public int MaxLength { get; }

    /// <inheritdoc />
    public ModuleInfo? Parent { get; }

    /// <inheritdoc />
    public string? Name
        => Aliases.Length > 0 ? Aliases[0] : null;

    /// <inheritdoc />
    public string FullName
        => $"{(Parent != null && Parent.Name != null ? $"{Parent.FullName} " : "")}{Name}";

    /// <inheritdoc />
    public float Score
        => GetScore();

    /// <inheritdoc />
    public bool IsRuntimeComponent
        => Parent == null;

    /// <inheritdoc />
    public bool IsSearchable
        => true;

    /// <inheritdoc />
    public bool IsDefault
        => Aliases.Length == 0;

    /// <inheritdoc />
    public bool HasArguments
        => Arguments.Length > 0;

    internal CommandInfo(ModuleInfo parent, StaticActivator invoker, string[] aliases, bool hasContext, ComponentConfiguration options)
        : this(parent, invoker, [], aliases, hasContext, options)
    {

    }

    internal CommandInfo(ModuleInfo parent, InstanceActivator activator, string[] aliases, ComponentConfiguration configuration)
        : this(parent, activator, [], aliases, true, configuration)
    {

    }

    internal CommandInfo(
        ModuleInfo? parent, IActivator invoker, ICondition[] conditions, string[] aliases, bool hasContext, ComponentConfiguration configuration)
    {
        var parameters = invoker.Target.BuildArguments(hasContext, configuration);

        (MinLength, MaxLength) = parameters.GetLength();

        Aliases = aliases;

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.IsRemainder && i != parameters.Length - 1)
                throw new NotSupportedException("Remainder arguments must be the last argument in the method signature.");
        }

        Activator = invoker;
        Parent = parent;

        var attributes = invoker.Target.GetAttributes(true);

        Attributes = attributes.ToArray();

        // 1: Runtime
        // 2: Attribute
        // 3: Parent
        Evaluators = ConditionEvaluator.CreateEvaluators(conditions)
            .Concat(ConditionEvaluator.CreateEvaluators(attributes.OfType<ICondition>()))
            .Concat(parent?.Evaluators ?? [])
            .ToArray();

        Arguments = parameters;
        HasRemainder = parameters.Any(x => x.IsRemainder);
    }

    /// <inheritdoc />
    public float GetScore()
    {
        var score = 1.0f;

        foreach (var argument in Arguments)
            score += argument.GetScore();

        score += Attributes.FirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

        return score;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is IScorable scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public bool Equals(IComponent? other)
        => other is CommandInfo info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override string ToString()
        => ToString(true);

    /// <inheritdoc cref="ToString()"/>
    /// <param name="withModuleInfo">Defines if the module information should be appended on the command level.</param>
    public string ToString(bool withModuleInfo)
        => $"{(withModuleInfo ? $"{Parent}." : "")}{Activator.Target.Name}{(Name != null ? $"['{Name}']" : "")}({string.Join<IArgument>(", ", Arguments)})";

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is CommandInfo info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override int GetHashCode()
        => Activator!.Target.GetHashCode();
}
