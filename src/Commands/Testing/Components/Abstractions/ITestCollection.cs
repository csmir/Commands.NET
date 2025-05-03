namespace Commands.Testing;

/// <summary>
///     
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITestCollection<T> : ICollection<T>, IEnumerable<T>
{
    /// <summary>
    ///     
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    //public int AddRange(IEnumerable<T> items);

    /// <summary>
    ///     
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    //public int RemoveRange(IEnumerable<T> items);
}
