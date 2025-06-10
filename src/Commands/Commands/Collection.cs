namespace Commands;

/// <summary>
///     A static class containing utility methods for working with collections.
/// </summary>
public static class Collection
{
    /// <summary>
    ///     Gets the first entry of the specified type, or <see langword="null"/> if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type to filter.</typeparam>
    /// <param name="values"></param>
    /// <returns>The first occurrence of <typeparamref name="T"/> in the collection if any exists, otherwise <see langword="null"/>.</returns>
    public static T? FirstOrDefault<T>(this IEnumerable values)
    {
        foreach (var entry in values)
        {
            if (entry is T tEntry)
                return tEntry;
        }

        return default;
    }

    /// <summary>
    ///     Checks if the collection contains an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to filter.</typeparam>
    /// <param name="values"></param>
    /// <returns><see langword="true"/> if a any <typeparamref name="T"/> was found, otherwise <see langword="false"/>.</returns>
    public static bool Contains<T>(this IEnumerable values)
    {
        foreach (var entry in values)
        {
            if (entry is T)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Gets all instances of the specified type, matching the provided predicate.
    /// </summary>
    /// <typeparam name="T">The type to filter.</typeparam>
    /// <param name="values"></param>
    /// <param name="predicate">The predicate which determines whether the component can be returned or not.</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all legible values of <typeparamref name="T"/> in the initial collection.</returns>
    public static IEnumerable<T> OfType<T>(this IEnumerable values, Predicate<T> predicate)
    {
        Assert.NotNull(predicate, nameof(predicate));

        foreach (var entry in values)
        {
            if (entry is T tEntry && predicate(tEntry))
                yield return tEntry;
        }
    }

    /// <summary>
    ///     Executes the provided action for each entry in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="values"></param>
    /// <param name="action">The action that will be executed on each record within the collection.</param>
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        Assert.NotNull(action, nameof(action));

        foreach (var entry in values)
            action(entry);
    }

    internal static void CopyTo<T>(ref T[] array, T item)
    {
        var newArray = new T[array.Length + 1];

        Array.Copy(array, newArray, array.Length);

        newArray[array.Length] = item;

        array = newArray;
    }

    internal static void CopyTo<T>(ref T[] array, T[] items)
    {
        var newArray = new T[array.Length + items.Length];

        Array.Copy(array, newArray, array.Length);

        var i = array.Length;

        foreach (var component in items)
            newArray[i++] = component;

        array = newArray;
    }
}
