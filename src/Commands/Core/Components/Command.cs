using Commands.Conditions;
using Commands.Parsing;
using System.Text;

namespace Commands;

/// <summary>
///     Contains information about a command that can be executed using an <see cref="IExecutionProvider"/>.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class Command : IComponent, IParameterCollection
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

    /// <inheritdoc />
    public int Position
        => (Parent?.Position ?? 0) + (Name == null ? 0 : 1);

    internal Command(CommandGroup parent, CommandStaticActivator invoker, string[] names, bool hasContext, ComponentConfiguration options)
        : this(parent, invoker, [], names, hasContext, options)
    {

    }

    internal Command(CommandGroup parent, CommandInstanceActivator activator, string[] names, ComponentConfiguration configuration)
        : this(parent, activator, [], names, false, configuration)
    {

    }

    internal Command(
        CommandGroup? parent, IActivator invoker, IExecuteCondition[] conditions, string[] names, bool hasContext, ComponentConfiguration configuration)
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
            .Concat(ConditionEvaluator.CreateEvaluators(attributes.OfType<IExecuteCondition>()))
            .Concat(parent?.Evaluators ?? [])
            .ToArray();

        Parameters = parameters;
        HasRemainder = parameters.Any(x => x.IsRemainder);
    }

    /// <summary>
    ///     Runs the execution steps for executing the command; Evaluating present conditions and invoking the command handler.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="ICallerContext"/> provided to this command.</typeparam>
    /// <param name="caller">The instance of the <see cref="ICallerContext"/> provided to this command.</param>
    /// <param name="args">The arguments to parse into valid command arguments.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> containing the result of the execution. If <see cref="IExecuteResult.Success"/> is <see langword="true"/>, the command has successfully been executed.</returns>
    public async ValueTask<IExecuteResult> Run<TContext>(TContext caller, ArgumentArray args, CommandOptions options)
        where TContext : ICallerContext
    {
        args.SetParseIndex(Position);

        object?[] parameters;

        if (!HasParameters && args.Length == 0)
            parameters = [];
        else if (MaxLength == args.Length || (MaxLength <= args.Length && HasRemainder) || (MaxLength > args.Length && MinLength <= args.Length))
        {
            var arguments = await this.Parse(caller, args, options).ConfigureAwait(false);

            parameters = new object[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].Success)
                    return ParseResult.FromError(new CommandParsingException(this, arguments[i].Exception));

                parameters[i] = arguments[i].Value;
            }
        }
        else
            return ParseResult.FromError(new CommandOutOfRangeException(this, args.Length));

        if (!options.SkipConditions)
        {
            foreach (var condition in Evaluators)
            {
                var checkResult = await condition.Evaluate(caller, this, options.Services, options.CancellationToken).ConfigureAwait(false);

                if (!checkResult.Success)
                    return ConditionResult.FromError(new CommandEvaluationException(this, checkResult.Exception));
            }
        }

        try
        {
            var value = Activator.Invoke(caller, this, parameters, options);

            return new InvokeResult(this, value, null);
        }
        catch (Exception exception)
        {
            return new InvokeResult(this, null, exception);
        }
    }

    /// <inheritdoc cref="GetFullName()" />
    /// <param name="includeArguments">Defines if the arguments of the command should be included in the output.</param>
    public string GetFullName(bool includeArguments)
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

        if (HasParameters && includeArguments)
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
    public string GetFullName()
        => GetFullName(true);

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
        => obj is ICommandSegment scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

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
        if (Activator is CommandDelegateActivator)
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

    // When a command is not yet bound to a parent, it can be bound when it is added to a CommandGroup. If it is added to a ComponentManager, it will not be bound.
    void IComponent.Bind(CommandGroup parent)
        => Parent ??= parent;
}
