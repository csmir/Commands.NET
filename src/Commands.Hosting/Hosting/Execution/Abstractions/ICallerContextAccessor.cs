namespace Commands.Hosting;

/// <summary>
///     
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICallerContextAccessor<out T>
    where T : class, ICallerContext
{
    /// <summary>
    ///     
    /// </summary>
    public T Caller { get; }
}
