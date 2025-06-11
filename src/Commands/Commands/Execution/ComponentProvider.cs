namespace Commands;

/// <summary>
///     A provider hosting a <see cref="ComponentTree"/> that can be executed through a pipeline. Begin using this provider by initializing it using any of the public constructors. 
///     This class can be implemented to provide custom behavior.
/// </summary>
public class ComponentProvider : IComponentProvider
{
    private static MethodInfo? _taskGetValue;

    /// <inheritdoc />
    public ComponentTree Components { get; }

    /// <inheritdoc />
    public event Action<IContext, IResult, Exception, IServiceProvider>? OnFailure;

    /// <inheritdoc />
    public event Action<IContext, IResult, IServiceProvider>? OnSuccess;

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/>.
    /// </summary>
    public ComponentProvider()
        => Components = [];

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/> using the provided <see cref="ComponentTree"/> as the source of components.
    /// </summary>
    /// <remarks>
    ///     The <paramref name="components"/> will be the source of <see cref="Components"/>, and can be mutated further at runtime to add or remove additional components.
    /// </remarks>
    /// <param name="components">A pre-initialized set of components which this provider should treat as the instance of <see cref="Components"/> targetted to find and execute commands from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="components"/> is <see langword="null"/>.</exception>
    public ComponentProvider(ComponentTree components)
    {
        Assert.NotNull(components, nameof(components));

        Components = components;
    }

    /// <inheritdoc />
    public virtual Task Execute<TContext>(TContext context, ExecutionOptions? options = null)
        where TContext : class, IContext
    {
        options ??= ExecutionOptions.Default;

        if (options.ExecuteAsynchronously)
        {
            _ = Work(context, options);

            return Task.CompletedTask;
        }

        return Work(context, options);
    }

    /// <summary>
    ///     A protected method that fires the command pipeline using the provided context and options, and handles the returned results in <see cref="Finalize{TContext}(TContext, IResult, ExecutionOptions)"/>.
    /// </summary>
    /// <remarks>
    ///      This method is called by the <see cref="Execute{TContext}"/> method after determining the execution approach, and can be overridden to provide custom behavior.
    /// </remarks>
    /// <typeparam name="TContext">The implementation type of <see cref="IContext"/> which represents the context of the execution.</typeparam>
    /// <param name="context">The implementation of <see cref="IContext"/> which represents the context of the execution.</param>
    /// <param name="options">The options used to customize the command execution pipeline in accordance to the context and requirements of execution.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the Work operation.</returns>
    protected virtual async Task Work<TContext>(TContext context, ExecutionOptions options)
        where TContext : class, IContext
    {
        options.ComponentProvider = this;

        IResult? result = null;

        var components = Components.Find(context.Arguments);

        foreach (var component in components)
        {
            if (component is Command command)
            {
                result = await command.Run(context, options).ConfigureAwait(false);

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
    ///     This method is called by the <see cref="Work{TContext}(TContext, ExecutionOptions)"/> method after the command execution has been completed, and can be overridden to provide custom behavior.
    /// </remarks>
    /// <typeparam name="TContext">The implementation type of <see cref="IContext"/> which represents the context of the execution.</typeparam>
    /// <param name="context">The implementation of <see cref="IContext"/> which represents the context of the execution.</param>
    /// <param name="result">The result yielded by the pipeline.</param>
    /// <param name="options">The options used to customize the command execution pipeline in accordance to the context and requirements of execution.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the Finalize operation.</returns>
#if NET8_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<>))]
#endif
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
            switch (invokeResult.ReturnValue)
            {
                case null: // (void)
                    break;

                case Task task:
                    await task.ConfigureAwait(false);

                    var taskType = task.GetType();

                    // If the task is a generic task, and the result is not a void task result, get the result and respond with it.
                    // Unfortunately we cannot do a type comparison on VoidTaskResult, because it is an internal corelib struct.
                    if (taskType.IsGenericType && taskType.GenericTypeArguments[0].Name != "VoidTaskResult")
                    {
                        _taskGetValue ??= taskType.GetProperty("Result")!.GetMethod;

                        var output = _taskGetValue?.Invoke(task, null);

                        if (output != null)
                            await AsyncContext.Respond(context, output).ConfigureAwait(false);
                    }
                    break;

                case object obj:
                    await AsyncContext.Respond(context, obj).ConfigureAwait(false);
                    break;
            }

            OnSuccess?.Invoke(context, result, options.ServiceProvider);
        }
        else
            OnFailure?.Invoke(context, result, Unfold(result.Exception)!, options.ServiceProvider);
    }
}
