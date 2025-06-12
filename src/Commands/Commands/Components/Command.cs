using Commands.Conditions;
using Commands.Parsing;
using Commands.Testing;
using System.Text;

namespace Commands;

/// <summary>
///     Contains information about a command that can be executed using an <see cref="IComponentProvider"/>.
/// </summary>
/// <remarks>
///     A <see cref="Command"/> is the target of an execution request. 
///     It is the final destination of the execution pipeline, which can be invoked using an <see cref="IComponentProvider"/> or by directly calling <see cref="Run{TContext}(TContext, ExecutionOptions)"/>. 
///     <br/>
///     A command can be added to a parent <see cref="CommandGroup"/>, or be added to a <see cref="ComponentTree"/> directly. When added to a group, the <see cref="Parent"/> property will be set, and it is not required to have a name.
///     Otherwise, the <see cref="Parent"/> will be <see langword="null"/> and <see cref="Names"/> requires a value.
/// </remarks>
[DebuggerDisplay("{ToString()}")]
public class Command : IComponent, IParameterCollection
{
    private bool _bound;

    /// <summary>
    ///     Gets all evaluations that this component should do during the execution process, determined by a set of defined <see cref="ICondition"/>'s pointing at the component.
    /// </summary>
    /// <remarks>
    ///     When this property is called by a child component, this property will inherit all evaluations from the component's <see cref="Parent"/> component(s).
    /// </remarks>
    public ConditionEvaluator[] Evaluators { get; private set; }

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
    ///     Gets the score of the command, which is used to set the priority of the command during execution. Higher scores take priority over lower scores.
    /// </summary>
    /// <remarks>
    ///     By default, a command has a score calculated by the following logic:
    ///     <list type="number">
    ///         <item>For each parameter in the signature, the score is incremented by <b>1</b>.</item>
    ///         <item>If a parameter is optional, the value is decremented by <b>0.5</b>.</item>
    ///         <item>If a parameter is remainder, the value is decremented by <b>0.25</b>.</item>
    ///         <item>If a parameter is nullable, the value is decremented by <b>0.25</b>.</item>
    ///         <item>If a parameter has a known type parser, the value is incremenented by <b>1</b>.</item>
    ///         <item>Further score incrementation from <see cref="PriorityAttribute"/> adds up to the total.</item>
    ///     </list>
    ///     The total value determines the score of the command, which is used to set the priority of the command during execution as stated above.
    /// </remarks>
    public float Score
        => GetScore();

    /// <inheritdoc cref="Command(Delegate, IEnumerable{ExecuteCondition}, string[], ComponentOptions?)"/>
    public Command(Delegate executionDelegate, params string[] names)
        : this(executionDelegate, [], names) { }

    /// <inheritdoc cref="Command(Delegate, IEnumerable{ExecuteCondition}, string[], ComponentOptions?)"/>
    public Command(Delegate executionDelegate, string[] names, ComponentOptions? options = null)
        : this(executionDelegate, [], names, options) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="Command"/>.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be ran when the command is executed.</param>
    /// <param name="conditions">The conditions bound to the command, which will determine whether it can execute or not.</param>
    /// <param name="names">The names used to discover this command during execution.</param>
    /// <param name="options">An optional configuration containing additional settings when creating this command.</param>
    /// <exception cref="ArgumentNullException">The provided delegate or conditions are <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The provided <paramref name="names"/> is <see langword="null"/> or does not match the <see cref="ComponentOptions.NameValidation"/> if any.</exception>
    /// <exception cref="ComponentFormatException">A <see cref="RemainderAttribute"/> is not placed on the last parameter of the target, <see cref="DeconstructAttribute"/> is defined on a non-deconstructible parameter type, or no <see cref="IParser"/> is available to represent a parameter type.</exception>
    public Command(Delegate executionDelegate, IEnumerable<ExecuteCondition> conditions, string[] names, ComponentOptions? options = null)
        : this(new CommandStaticActivator(executionDelegate.Method, executionDelegate.Target), options ??= ComponentOptions.Default)
    {
        Assert.NotNull(conditions, nameof(conditions));
        Assert.NotNullOrInvalid(names, options.NameValidation, nameof(names));

        Names = names;
        Ignore = false;
        Evaluators = ConditionEvaluator.CreateEvaluators(conditions);

        options.BuildCompleted?.Invoke(this);
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="Command"/>.
    /// </summary>
    /// <param name="executionMethod">The method to run when the command is executed.</param>
    /// <param name="options">An optional configuration containing additional settings when creating this command.</param>
    /// <exception cref="ArgumentNullException">The provided method is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The provided <paramref name="executionMethod"/> defines names, but those names do not match the provided <see cref="ComponentOptions.NameValidation"/>.</exception>
    /// <exception cref="ComponentFormatException">A <see cref="RemainderAttribute"/> is not placed on the last parameter of the target, <see cref="DeconstructAttribute"/> is defined on a non-deconstructible parameter type, or no <see cref="IParser"/> is available to represent a parameter type.</exception>
    public Command(MethodInfo executionMethod, ComponentOptions? options = null)
        : this(executionMethod.IsStatic ? new CommandStaticActivator(executionMethod) : new CommandInstanceActivator(executionMethod), options ??= ComponentOptions.Default)
    {
        var names = Attributes.FirstOrDefault<INameBinding>()?.Names ?? [];
        
        Assert.NotNullOrInvalid(names, options.NameValidation, nameof(INameBinding));

        Names = names;
        Ignore = Attributes.Any(x => x is IgnoreAttribute);
        Evaluators = ConditionEvaluator.CreateEvaluators(Attributes.OfType<ICondition>());

        options.BuildCompleted?.Invoke(this);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Command(IActivator activator, ComponentOptions options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        var parameters = Utilities.GetParameters(activator, options);
        var attributes = activator.Target.GetAttributes(true);

        Attributes = [.. attributes];
        Activator = activator;

        (MinLength, MaxLength) = Utilities.GetLength(parameters);

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.IsRemainder && i != parameters.Length - 1)
                throw new ComponentFormatException("Remainder-marked parameters must be the last parameter in the parameter list of a command.");
        }

        Parameters = parameters;
        HasRemainder = parameters.Any(x => x.IsRemainder);
    }

    /// <summary>
    ///     Runs the execution steps for executing the command; Evaluating present conditions and invoking the command handler.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="IContext"/> provided to this command.</typeparam>
    /// <param name="context">The instance of the <see cref="IContext"/> provided to this command.</param>
    /// <param name="options">A collection of options that determines pipeline logic.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> containing the result of the execution. If <see cref="IResult.Success"/> is <see langword="true"/>, the command has successfully been executed.</returns>
    public async ValueTask<IResult> Run<TContext>(TContext context, ExecutionOptions options)
        where TContext : class, IContext
    {
        var args = context.Arguments;

        args.SetIndex(Position);

        object?[] parameters;

        if (!HasParameters && args.RemainingLength == 0)
            parameters = [];
        else if (MaxLength == args.RemainingLength || (MaxLength <= args.RemainingLength && HasRemainder) || (MaxLength > args.RemainingLength && MinLength <= args.RemainingLength))
        {
            var arguments = await Utilities.ParseParameters(this, context, args, options).ConfigureAwait(false);

            parameters = new object[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].Success)
                    return ParseResult.FromError(new CommandParsingException(this, arguments[i].Exception));

                parameters[i] = arguments[i].Value;
            }
        }
        else
            return ParseResult.FromError(new CommandOutOfRangeException(this, args.RemainingLength));

        if (!options.SkipConditions)
        {
            foreach (var evaluator in Evaluators)
            {
                var checkResult = await evaluator.Evaluate(context, this, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);

                if (!checkResult.Success)
                    return ConditionResult.FromError(new CommandEvaluationException(this, checkResult.Exception));
            }
        }

        try
        {
            var value = Activator.Invoke(context, this, parameters, options);

            return new InvokeResult(this, value, null);
        }
        catch (Exception exception)
        {
            return new InvokeResult(this, null, exception);
        }
    }

    /// <summary>
    ///     Tests the target command using the provided <paramref name="contextCreationDelegate"/> function to create the context for each individual execution.
    /// </summary>
    /// <remarks>
    ///     Define tests on commands using the <see cref="TestAttribute"/> attribute on the command method or delegate.
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="IContext"/> that this test sequence should use to test with.</typeparam>
    /// <param name="contextCreationDelegate">A delegate that yields an implementation of <typeparamref name="TContext"/> based on the input value for every new test.</param>
    /// <param name="options">A collection of options that determine how every test against this command is ran.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing an <see cref="IEnumerable{T}"/> with the result of every test yielded by this operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextCreationDelegate"/> is <see langword="null"/>.</exception>
    public async ValueTask<IEnumerable<TestResult>> Test<TContext>(Func<string, TContext> contextCreationDelegate, ExecutionOptions? options = null)
        where TContext : class, IContext
    {
        static TestResult Compare(ITest test, TestResultType targetType, Exception exception)
        {
            return test.ShouldEvaluateTo == targetType
                ? TestResult.FromSuccess(test, test.ShouldEvaluateTo)
                : TestResult.FromError(test, test.ShouldEvaluateTo, targetType, exception);
        }

        Assert.NotNull(contextCreationDelegate, nameof(contextCreationDelegate));

        options ??= ExecutionOptions.Default;

        if (options.ExecuteAsynchronously)
            options.ExecuteAsynchronously = false;

        var tests = Attributes.OfType<ITest>().ToArray();

        var results = new TestResult[tests.Length];

        for (var i = 0; i < tests.Length; i++)
        {
            var test = tests[i];

            var fullName = string.IsNullOrWhiteSpace(test.Arguments)
                ? GetFullName(false)
                : GetFullName(false) + ' ' + test.Arguments;

            var runResult = await Run(contextCreationDelegate(fullName), options).ConfigureAwait(false);

            results[i] = runResult.Exception switch
            {
                null => Compare(test, TestResultType.Success, new InvalidOperationException("The command was expected to fail, but it succeeded.")),

                CommandParsingException => Compare(test, TestResultType.ParseFailure, runResult.Exception),
                CommandEvaluationException => Compare(test, TestResultType.ConditionFailure, runResult.Exception),
                CommandOutOfRangeException => Compare(test, TestResultType.MatchFailure, runResult.Exception),

                _ => Compare(test, TestResultType.InvocationFailure, runResult.Exception),
            };
        }

        return results;
    }

    /// <inheritdoc cref="GetFullName()" />
    /// <param name="includeArguments">Defines if the arguments of the command should be included in the output.</param>
    public string GetFullName(bool includeArguments)
    {
        var sb = new StringBuilder();

        if (Parent?.Name != null)
        {
            sb.Append(Parent.GetFullName());

            if (Name != null)
            {
                sb.Append(' ');
                sb.Append(Name);
            }
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

        foreach (var parameter in Parameters)
            score += parameter.GetScore();

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

    #region Internals

    int IComparable.CompareTo(object? obj)
        => obj is IComponent component ? CompareTo(component) : -1;

    void IComponent.Bind(ComponentSet parent)
    {
        if (_bound)
            throw new ComponentFormatException($"{this} is already bound to a {(Parent != null ? nameof(CommandGroup) : nameof(ComponentSet))}. Remove this component from the parent set before adding it to another.");

        if (parent is CommandGroup group)
        {
            Parent = group;

            // We set the evaluators again to ensure that the parent conditions are included in the evaluation, if the parent has any.
            Evaluators = ConditionEvaluator.CreateEvaluators([.. Attributes.OfType<ICondition>(), .. Parent.GetConditions()]);
        }

        _bound = true;
    }

    void IComponent.Unbind()
    {
        _bound = false;
        Parent = null;
        Evaluators = ConditionEvaluator.CreateEvaluators(Attributes.OfType<ICondition>());
    }

    #endregion
}
