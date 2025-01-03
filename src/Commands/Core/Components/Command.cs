using Commands.Conditions;
using System.Reflection;

namespace Commands;

/// <summary>
///     Contains information about a command that can be executed using an <see cref="IComponentTree"/>.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class Command : IComponent, IParameterCollection
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
    public ICommandParameter[] Parameters { get; }

    /// <inheritdoc />
    public bool HasRemainder { get; }

    /// <inheritdoc />
    public int MinLength { get; }

    /// <inheritdoc />
    public int MaxLength { get; }

    /// <inheritdoc />
    public CommandGroup? Parent { get; }

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
    public bool IsSearchable
        => true;

    /// <inheritdoc />
    public bool IsDefault
        => Aliases.Length == 0;

    /// <inheritdoc />
    public bool HasParameters
        => Parameters.Length > 0;

    internal Command(CommandGroup parent, StaticCommandActivator invoker, string[] aliases, bool hasContext, ComponentConfiguration options)
        : this(parent, invoker, [], aliases, hasContext, options)
    {

    }

    internal Command(CommandGroup parent, InstanceCommandActivator activator, string[] aliases, ComponentConfiguration configuration)
        : this(parent, activator, [], aliases, false, configuration)
    {

    }

    internal Command(
        CommandGroup? parent, IActivator invoker, ICondition[] conditions, string[] aliases, bool hasContext, ComponentConfiguration configuration)
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

        Parameters = parameters;
        HasRemainder = parameters.Any(x => x.IsRemainder);
    }

    /// <inheritdoc />
    public float GetScore()
    {
        var score = 1.0f;

        foreach (var argument in Parameters)
            score += argument.GetScore();

        score += Attributes.FirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

        return score;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is IScorable scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public bool Equals(IComponent? other)
        => other is Command info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override string ToString()
        => ToString(true);

    /// <inheritdoc cref="ToString()"/>
    /// <param name="withModuleInfo">Defines if the module information should be appended on the command level.</param>
    public string ToString(bool withModuleInfo)
        => $"{(withModuleInfo ? $"{Parent}." : "")}{Activator.Target.Name}{(Name != null ? $"['{Name}']" : "")}({string.Join<ICommandParameter>(", ", Parameters)})";

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Command info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    /// <inheritdoc cref="Create(Delegate, string[], ICondition[], ComponentConfiguration?)"/>
    public static Command Create(Delegate executionDelegate, string[] aliases, ComponentConfiguration? configuration = null)
        => Create(executionDelegate, aliases, [], configuration);

    /// <summary>
    ///     Creates a new command from a delegate. If a method within a module is provided, it will not root itself to its parent, and will instead be a top-level command on its own.
    /// </summary>
    /// <param name="executionDelegate">The delegate which contains an action executed when the command is triggered by a caller.</param>
    /// <param name="aliases">A set of names by which the command will be able to be executed.</param>
    /// <param name="conditions">A set of conditions that should be evaluated before the command is executed.</param>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <returns>A new instance of <see cref="Command"/> being an action that can be formed based on provided input.</returns>
    public static Command Create(Delegate executionDelegate, string[] aliases, ICondition[] conditions, ComponentConfiguration? configuration = null)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        configuration ??= new ComponentConfiguration();

        Assert.Aliases(aliases, configuration, false);

        var hasContext = executionDelegate.Method.HasContext();

        return new Command(null, new DelegateCommandActivator(executionDelegate.Method, executionDelegate.Target, hasContext), conditions, aliases, hasContext, configuration);
    }
}
