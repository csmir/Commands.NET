namespace Commands.Hosting;

/// <summary>
///     Provides a mechanism for accessing the <see cref="ICallerContext"/> of the current execution pipeline.
/// </summary>
/// <typeparam name="TContext">The implementation of <see cref="ICallerContext"/> this accessor should return. If the current context is no implementation of <typeparamref name="TContext"/>, getting <see cref="Caller"/> will throw an exception.</typeparam>
public interface IContextAccessor<out TContext>
    where TContext : ICallerContext
{
    /// <summary>
    ///     Gets the <see cref="ICallerContext"/> of the current execution pipeline.
    /// </summary>
    public TContext Caller { get; }
}
