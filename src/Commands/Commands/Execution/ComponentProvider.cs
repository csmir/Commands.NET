namespace Commands;

/// <summary>
///     A provider hosting a <see cref="ComponentTree"/> that can be executed through a pipeline. Begin using this provider by initializing it using any of the public constructors. 
///     This class can be implemented to provide custom behavior.
/// </summary>
public class ComponentProvider : IComponentProvider
{
    /// <inheritdoc />
    public ComponentTree Components { get; }

    /// <inheritdoc />
    public event Func<IContext, IResult, Exception, IServiceProvider, Task>? OnFailure;

    /// <inheritdoc />
    public event Func<IContext, IResult, IServiceProvider, Task>? OnSuccess;

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/>.
    /// </summary>
    public ComponentProvider()
        => Components = [];

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/> using the provided <see cref="IComponent"/> implementations as the components to use.
    /// </summary>
    /// <remarks>
    ///     This constructor will create a new <see cref="ComponentTree"/> using the provided <paramref name="components"/> as the source of <see cref="Components"/>. See <see cref="ComponentProvider.ComponentProvider(ComponentTree)"/> for more.
    /// </remarks>
    /// <param name="components">A collection of components which this provider should treat as the instance of <see cref="Components"/> to find and execute commands from.</param>
    public ComponentProvider(IEnumerable<IComponent> components)
        : this([.. components])
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/> using the provided <see cref="ComponentTree"/> as the source of components.
    /// </summary>
    /// <remarks>
    ///     The <paramref name="components"/> will be the source of <see cref="Components"/>, and can be mutated further at runtime to add or remove additional components.
    /// </remarks>
    /// <param name="components">A pre-initialized set of components which this provider should treat as the instance of <see cref="Components"/> to find and execute commands from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="components"/> is <see langword="null"/>.</exception>
    public ComponentProvider(ComponentTree components)
    {
        if (components == null)
            throw new ArgumentNullException(nameof(components));

        Components = components;
    }

    /// <inheritdoc />
    public virtual async Task Execute<TContext>(TContext context, ExecutionOptions? options = null)
        where TContext : class, IContext
    {
        options ??= ExecutionOptions.Default;

        IResult? result = null;

        var components = Components.Find(context.Arguments);

        foreach (var component in components)
        {
            if (component is Command command)
            {
                result = await command.Run(context, options, this).ConfigureAwait(false);

                if (result.Success)
                    break;
            }

            result ??= new SearchResult(new CommandRouteIncompleteException(component));
        }

        result ??= new SearchResult(new CommandNotFoundException());

        await Finalize(context, result, options).ConfigureAwait(false);
    }

    /// <summary>
    ///     A protected method that finalizes the command execution by handling the result of the command execution and invoking the appropriate success or failure handlers.
    /// </summary>
    /// <remarks>
    ///     This method is called by the <see cref="Execute{TContext}(TContext, ExecutionOptions?)"/> method after the command execution has been completed, and can be overridden to provide custom behavior.
    /// </remarks>
    /// <typeparam name="TContext">The implementation type of <see cref="IContext"/> which represents the context of the execution.</typeparam>
    /// <param name="context">The implementation of <see cref="IContext"/> which represents the context of the execution.</param>
    /// <param name="result">The result yielded by the pipeline.</param>
    /// <param name="options">The options used to customize the command execution pipeline in accordance to the context and requirements of execution.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the Finalize operation.</returns>
    protected virtual async Task Finalize<TContext>(TContext context, IResult result, ExecutionOptions options)
        where TContext : class, IContext
    {
        static Exception? Unfold(Exception? exception)
        {
            if (exception?.InnerException != null)
                return Unfold(exception.InnerException);

            return exception;
        }

        if (result.Success && result is InvokeResult invokeResult)
        {
            // The return value has already been awaited and unwrapped by the command. Void, Task and ValueTask commands have no value.
            if (invokeResult.ReturnValue != null)
            {
                try
                {
                    await AsyncContext.Respond(context, invokeResult.ReturnValue).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await InvokeFailure(context, new InvokeResult(invokeResult.Command, invokeResult.ReturnValue, exception), Unfold(exception)!, options.ServiceProvider).ConfigureAwait(false);

                    return;
                }
            }

            await InvokeSuccess(context, result, options.ServiceProvider).ConfigureAwait(false);
        }
        else
            await InvokeFailure(context, result, Unfold(result.Exception)!, options.ServiceProvider).ConfigureAwait(false);
    }

    // Awaits every subscriber in order. Invoking the delegate directly would only return the task of the last subscriber.
    private async Task InvokeFailure(IContext context, IResult result, Exception exception, IServiceProvider services)
    {
        var handlers = OnFailure;

        if (handlers == null)
            return;

        foreach (var handler in handlers.GetInvocationList())
            await ((Func<IContext, IResult, Exception, IServiceProvider, Task>)handler)(context, result, exception, services).ConfigureAwait(false);
    }

    // Awaits every subscriber in order. Invoking the delegate directly would only return the task of the last subscriber.
    private async Task InvokeSuccess(IContext context, IResult result, IServiceProvider services)
    {
        var handlers = OnSuccess;

        if (handlers == null)
            return;

        foreach (var handler in handlers.GetInvocationList())
            await ((Func<IContext, IResult, IServiceProvider, Task>)handler)(context, result, services).ConfigureAwait(false);
    }
}
