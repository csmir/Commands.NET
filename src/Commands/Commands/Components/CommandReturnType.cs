namespace Commands;

/// <summary>
///     Represents the return type of a command method, including whether it is awaitable and how to retrieve its result.
/// </summary>
public readonly struct CommandReturnType
{
    private readonly CommandReturnKind _kind;

    // Task<T>.Result, or the Result of the Task<T> returned by ValueTask<T>.AsTask.
    private readonly MethodInfo? _getResult;

    // ValueTask<T>.AsTask.
    private readonly MethodInfo? _asTask;

    /// <summary>
    ///     Gets whether the command method's return type is a <see cref="Task"/>, <see cref="ValueTask"/>, <see cref="Task{T}"/>, or <see cref="ValueTask{T}"/>.
    /// </summary>
    public bool IsAwaitable
        => _kind != CommandReturnKind.Value;

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandReturnType"/> based on the return type of the given method.
    /// </summary>
    /// <param name="target">The method to analyze the return type of.</param>
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "The public members of Task<> and ValueTask<> are preserved through the DynamicDependency attributes.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(ValueTask<>))]
#endif
    public CommandReturnType(MethodBase target)
    {
        _kind = CommandReturnKind.Value;

        if (target is not MethodInfo method)
            return;

        var type = method.ReturnType;

        if (type == typeof(Task))
            _kind = CommandReturnKind.Task;

        else if (type == typeof(ValueTask))
            _kind = CommandReturnKind.ValueTask;

        else if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();

            if (definition == typeof(Task<>))
            {
                _kind = CommandReturnKind.TaskOfT;
                _getResult = type.GetProperty("Result")!.GetMethod;
            }
            else if (definition == typeof(ValueTask<>))
            {
                _kind = CommandReturnKind.ValueTaskOfT;
                _asTask = type.GetMethod("AsTask")!;
                _getResult = _asTask.ReturnType.GetProperty("Result")!.GetMethod;
            }
        }
    }

    /// <summary>
    ///     Awaits the given value if it is a Task or ValueTask, and returns the result if it is a <see cref="Task{T}"/> or <see cref="ValueTask{T}"/>.
    /// </summary>
    /// <param name="value">The value to await and get the result from.</param>
    /// <returns>The result of the awaited value, or null if the value does not produce a result.</returns>
    public async ValueTask<object?> GetAsyncResult(object? value)
    {
        switch (_kind)
        {
            case CommandReturnKind.Task:
                await ((Task)value!).ConfigureAwait(false);
                return null;
            case CommandReturnKind.ValueTask:
                await ((ValueTask)value!).ConfigureAwait(false);
                return null;
            case CommandReturnKind.TaskOfT:
                {
                    var task = (Task)value!;

                    await task.ConfigureAwait(false);

                    return _getResult!.Invoke(task, null);
                }
            case CommandReturnKind.ValueTaskOfT:
                {
                    var task = (Task)_asTask!.Invoke(value, null)!;

                    await task.ConfigureAwait(false);

                    return _getResult!.Invoke(task, null);
                }
            default:
                return value;
        }
    }
}
