﻿using Commands.Conditions;
using Commands.Parsing;
using System.Text;

namespace Commands;

/// <summary>
///     Contains information about a command that can be executed using an <see cref="IExecutionProvider"/>.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class Command : IComponent, IParameterCollection
{
    /// <summary>
    ///     Gets all evaluations that this component should do during the execution process, determined by a set of defined <see cref="IExecuteCondition"/>'s pointing at the component.
    /// </summary>
    /// <remarks>
    ///     When this property is called by a child component, this property will inherit all evaluations from the component's <see cref="Parent"/> component(s).
    /// </remarks>
    public ConditionEvaluator[] Evaluators { get; }

    /// <inheritdoc />
    public CommandGroup? Parent { get; private set; }

    /// <inheritdoc />
    public IActivator Activator { get; }

    /// <inheritdoc />
    public string[] Names { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public bool Ignore { get; }

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

    /// <summary>
    ///     Initializes a new instance of <see cref="Command"/> with the provided execution delegate, conditions, names, configuration, and parent group.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be ran when the command is executed.</param>
    /// <param name="names">The names used to discover this command during execution.</param>
    public Command(Delegate executionDelegate, params string[] names)
        : this(executionDelegate, [], names) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="Command"/> with the provided execution delegate, conditions, names, configuration, and parent group.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be ran when the command is executed.</param>
    /// <param name="names">The names used to discover this command during execution.</param>
    /// <param name="configuration">An optional configuration containing additional settings when creating this command.</param>
    public Command(Delegate executionDelegate, string[] names, ComponentConfiguration? configuration = null)
        : this(executionDelegate, [], names, configuration) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="Command"/> with the provided execution delegate, conditions, names, configuration, and parent group.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be ran when the command is executed.</param>
    /// <param name="conditions">The conditions bound to the command, which will determine whether it can execute or not.</param>
    /// <param name="names">The names used to discover this command during execution.</param>
    /// <param name="configuration">An optional configuration containing additional settings when creating this command.</param>
    public Command(Delegate executionDelegate, IEnumerable<ExecuteCondition> conditions, string[] names, ComponentConfiguration? configuration = null)
        : this(new CommandStaticActivator(executionDelegate.Method, executionDelegate.Target), configuration ?? ComponentConfiguration.Empty)
    {
        Names = names;
        Ignore = false;

        // Delegate commands do not support modularity approach or inheriting conditions from parent groups.
        Evaluators = ConditionEvaluator.CreateEvaluators(conditions);
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="Command"/> with the provided execution method, configuration, and parent group.
    /// </summary>
    /// <param name="executionMethod">The method to run when the command is executed.</param>
    /// <param name="parent">The parent of this command, if any. Irrespective of this value being set, the command can still be added to groups at any time. This parameter will however, inherit the execution conditions from the parent.</param>
    /// <param name="configuration">An optional configuration containing additional settings when creating this command.</param>
    public Command(MethodInfo executionMethod, CommandGroup? parent = null, ComponentConfiguration? configuration = null)
        : this(executionMethod.IsStatic ? new CommandStaticActivator(executionMethod) : new CommandInstanceActivator(executionMethod), configuration ?? ComponentConfiguration.Empty)
    {
        Names = Attributes.FirstOrDefault<NameAttribute>()?.Names ?? [];
        Ignore = Attributes.Contains<IgnoreAttribute>();

        if (parent != null)
            Evaluators = ConditionEvaluator.CreateEvaluators([.. Attributes.OfType<IExecuteCondition>(), .. parent.GetConditions()]);
        else
            Evaluators = ConditionEvaluator.CreateEvaluators(Attributes.OfType<IExecuteCondition>());

        Parent = parent;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Command(IActivator activator, ComponentConfiguration configuration)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        var parameters = ComponentUtilities.GetParameters(activator, configuration);
        var attributes = activator.Target.GetAttributes(true);

        Attributes = [.. attributes];
        Activator = activator;

        (MinLength, MaxLength) = parameters.GetLength();

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.IsRemainder && i != parameters.Length - 1)
                throw new NotSupportedException("Remainder arguments must be the last argument in the method signature.");
        }

        Parameters = parameters;
        HasRemainder = parameters.Any(x => x.IsRemainder);
    }

    /// <summary>
    ///     Runs the execution steps for executing the command; Evaluating present conditions and invoking the command handler.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="ICallerContext"/> provided to this command.</typeparam>
    /// <param name="caller">The instance of the <see cref="ICallerContext"/> provided to this command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> containing the result of the execution. If <see cref="IResult.Success"/> is <see langword="true"/>, the command has successfully been executed.</returns>
    public async ValueTask<IResult> Run<TContext>(TContext caller, CommandOptions options)
        where TContext : class, ICallerContext
    {
        var args = caller.Arguments;

        args.SetParseIndex(Position);

        object?[] parameters;

        if (!HasParameters && args.AvailableLength == 0)
            parameters = [];
        else if (MaxLength == args.AvailableLength || (MaxLength <= args.AvailableLength && HasRemainder) || (MaxLength > args.AvailableLength && MinLength <= args.AvailableLength))
        {
            var arguments = await ComponentUtilities.Parse(this, caller, args, options).ConfigureAwait(false);

            parameters = new object[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].Success)
                    return ParseResult.FromError(new CommandParsingException(this, arguments[i].Exception));

                parameters[i] = arguments[i].Value;
            }
        }
        else
            return ParseResult.FromError(new CommandOutOfRangeException(this, args.AvailableLength));

        if (!options.SkipConditions)
        {
            foreach (var condition in Evaluators)
            {
                var checkResult = await condition.Evaluate(caller, this, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);

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
    public int CompareTo(IComponent? component)
        => GetScore().CompareTo(component?.GetScore());

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

    int IComparable.CompareTo(object? obj)
        => obj is IComponent component ? CompareTo(component) : -1;

    #region Initializers

    /// <inheritdoc cref="From(Delegate, string[])"/>
    public static CommandProperties With
        => new();

    /// <inheritdoc cref="From(Delegate, string[])"/>
    public static CommandProperties From(params string[] names)
        => new CommandProperties().Names(names);

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="Command"/>.
    /// </summary>
    /// <param name="executionDelegate">The command body that should be executed when this command is invoked.</param>
    /// <param name="names">A set of names this command be discovered by.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static CommandProperties From(Delegate executionDelegate, params string[] names)
        => new CommandProperties().Delegate(executionDelegate).Names(names);

    #endregion
}
