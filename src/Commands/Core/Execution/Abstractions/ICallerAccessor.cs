namespace Commands.Core.Execution.Abstractions;

/// <summary>
///     A mechanism that can extract the caller consumed within the command pipeline in a scoped scenario.
/// </summary>
/// <typeparam name="T">The <see cref="ICallerContext"/> implementation that this accessor should yield to the scope.</typeparam>
public interface ICallerAccessor<out T>
    where T : ICallerContext
{
    /// <summary>
    ///     Gets the caller that requested this command to be executed.
    /// </summary>
    public T Caller { get; }
}
