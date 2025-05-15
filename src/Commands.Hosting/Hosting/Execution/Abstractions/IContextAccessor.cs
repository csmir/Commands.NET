namespace Commands.Hosting;

/// <summary>
///     Provides a mechanism for accessing the <see cref="ICallerContext"/> of the current execution pipeline.
/// </summary>
/// <typeparam name="TCaller">The implementation of <see cref="ICallerContext"/> this accessor should return. If the current context is no implementation of <typeparamref name="TCaller"/>, getting <see cref="Caller"/> will throw an exception.</typeparam>
public interface IContextAccessor<out TCaller>
    where TCaller : ICallerContext
{
    /// <summary>
    ///     Gets the <see cref="ICallerContext"/> of the current execution pipeline.
    /// </summary>
    public TCaller Caller { get; }
}
