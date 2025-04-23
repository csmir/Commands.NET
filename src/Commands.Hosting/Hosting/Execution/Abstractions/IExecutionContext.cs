using System.Diagnostics.CodeAnalysis;

namespace Commands.Hosting;

/// <summary>
///     
/// </summary>
public interface IExecutionContext : IDisposable
{
    /// <summary>
    ///     
    /// </summary>
    public CancellationTokenSource CancellationSource { get; }

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="caller"></param>
    /// <returns></returns>
    public bool TryGetCaller<T>([NotNullWhen(true)] out T? caller)
        where T : class, ICallerContext;
}
