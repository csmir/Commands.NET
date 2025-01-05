using Commands.Conditions;
using System.Text;

namespace Commands;

/// <summary>
///     Contains information about a command that can be executed using an <see cref="IExecutionProvider"/>.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class Command : IComponent, ICommandSegment, IParameterCollection
{
    /// <inheritdoc />
    public CommandGroup? Parent { get; private set; }

    /// <inheritdoc />
    public IActivator Activator { get; }

    /// <inheritdoc />
    public string[] Names { get; }

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
    public string? Name
        => Names.Length > 0 ? Names[0] : null;

    /// <inheritdoc />
    public string FullName
        => GetFullName();

    /// <inheritdoc />
    public float Score
        => GetScore();

    /// <inheritdoc />
    public bool IsSearchable
        => true;

    /// <inheritdoc />
    public bool IsDefault
        => Names.Length == 0;

    /// <inheritdoc />
    public bool HasParameters
        => Parameters.Length > 0;

    internal Command(CommandGroup parent, StaticCommandActivator invoker, string[] names, bool hasContext, ComponentConfiguration options)
        : this(parent, invoker, [], names, hasContext, options)
    {

    }

    internal Command(CommandGroup parent, InstanceCommandActivator activator, string[] names, ComponentConfiguration configuration)
        : this(parent, activator, [], names, false, configuration)
    {

    }

    internal Command(
        CommandGroup? parent, IActivator invoker, ICondition[] conditions, string[] names, bool hasContext, ComponentConfiguration configuration)
    {
        var parameters = invoker.Target.BuildArguments(hasContext, configuration);

        (MinLength, MaxLength) = parameters.GetLength();

        Names = names;

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

    /// <summary>
    ///     Runs the execution steps for executing the command; Evaluating present conditions and invoking the command handler.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ICallerContext"/> provided to this command.</typeparam>
    /// <param name="caller">The instance of the <see cref="ICallerContext"/> provided to this command.</param>
    /// <param name="arguments">The arguments provided to this command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> containing the result of the execution. If <see cref="IExecuteResult.Success"/> is <see langword="true"/>, the command has successfully been executed.</returns>
    public async ValueTask<IExecuteResult> Run<T>(T caller, object?[] arguments, CommandOptions options)
        where T : ICallerContext
    {
        if (!options.SkipConditions)
        {
            foreach (var condition in Evaluators)
            {
                var checkResult = await condition.Evaluate(caller, this, options.Services, options.CancellationToken);

                if (!checkResult.Success)
                    return checkResult;
            }
        }

        try
        {
            var value = Activator.Invoke(caller, this, arguments, options);

            return InvokeResult.FromSuccess(this, value);
        }
        catch (Exception exception)
        {
            return InvokeResult.FromError(this, exception);
        }
    }

    /// <inheritdoc />
    public string GetFullName()
    {
        var sb = new StringBuilder();

        if (Parent != null && Parent.Name != null)
        {
            sb.Append(Parent.GetFullName());

            if (Name != null)
                sb.Append(' ');

            sb.Append(Name);
        }
        else if (Name != null)
            sb.Append(Name);

        if (HasParameters)
        {
            foreach (var parameter in Parameters)
            {
                sb.Append(' ');
                sb.Append(parameter.GetFullName());
            }
        }

        return sb.ToString();
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
    public bool HasAttribute<T>()
        where T : Attribute
        => Attributes.Contains<T>(true);

    /// <inheritdoc />
    public T? GetAttribute<T>(T? defaultValue = default)
        where T : Attribute
        => Attributes.FirstOrDefault<T>() ?? defaultValue;

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is ICommandSegment scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public bool Equals(IComponent? other)
        => other is Command info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override string ToString()
        => ToString(true);

    /// <inheritdoc cref="ToString()"/>
    /// <param name="withModuleInfo">Defines if the module information should be appended on the command level.</param>
    public string ToString(bool withModuleInfo)
    {
        var sb = new StringBuilder();

        if (withModuleInfo && Parent != null)
        {
            sb.Append(Parent.ToString());
            sb.Append('.');
        }

        // If the activator is a delegate variant, the target of the activator is in most circumstances, a generated signature which is not reader-friendly.
        // We instead will simply append the word "Command" to the output.
        if (Activator is DelegateCommandActivator)
            sb.Append("Command");
        else
            sb.Append(Activator.Target.Name);

        if (Name != null)
        {
            sb.Append("['");
            sb.Append(Name);
            sb.Append("']");
        }

        sb.Append('(');
        sb.Append(string.Join<ICommandParameter>(", ", Parameters));
        sb.Append(')');

        return sb.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Command info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    // When a command is not yet bound to a parent, it can be bound when it is added to a CommandGroup. If it is added to a ComponentTree, it will not be bound.
    void IComponent.Bind(CommandGroup parent)
        => Parent ??= parent;

    /// <inheritdoc cref="Create(Delegate, string[], ICondition[], ComponentConfiguration?)"/>
    public static Command Create(Delegate executionDelegate, params string[] names)
        => Create(executionDelegate, names, []);

    /// <inheritdoc cref="Create(Delegate, string[], ICondition[], ComponentConfiguration?)"/>
    public static Command Create(Delegate executionDelegate, string[] names, ComponentConfiguration? configuration = null)
        => Create(executionDelegate, names, [], configuration);

    /// <summary>
    ///     Creates a new command from a delegate. This command will be considered top-level until it is added manually to a <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="executionDelegate">The delegate which contains an action executed when the command is triggered by a caller.</param>
    /// <param name="names">A set of names by which the command will be able to be executed.</param>
    /// <param name="conditions">A set of conditions that should be evaluated before the command is executed.</param>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <returns>A new instance of <see cref="Command"/> being an action that can be formed based on provided input.</returns>
    public static Command Create(Delegate executionDelegate, string[] names, ICondition[] conditions, ComponentConfiguration? configuration = null)
    {
        Assert.NotNull(executionDelegate, nameof(executionDelegate));

        configuration ??= ComponentConfiguration.Default;

        Assert.Names(names, configuration, false);

        var hasContext = executionDelegate.Method.HasContext();

        return new Command(null, new DelegateCommandActivator(executionDelegate.Method, executionDelegate.Target, hasContext), conditions, names, hasContext, configuration);
    }
}
