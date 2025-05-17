namespace Commands;

/// <summary>
///     A static class containing methods for working with collections.
/// </summary>
public static class CollectionUtilities
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
        foreach (var entry in values)
        {
            if (entry is T tEntry && predicate(tEntry))
                yield return tEntry;
        }
    }

    // This method is used to add a component to the array of components with low allocation overhead.
    internal static void Add(ref IComponent[] array, IComponent component)
    {
        var newArray = new IComponent[array.Length + 1];

        Array.Copy(array, newArray, array.Length);

        newArray[array.Length] = component;

        array = newArray;
    }

    // This method is used to add a range of components to the array of components with low allocation overhead.
    internal static void AddRange(ref IComponent[] array, IComponent[] components)
    {
        var newArray = new IComponent[array.Length + components.Length];

        Array.Copy(array, newArray, array.Length);

        var i = array.Length;

        foreach (var component in components)
            newArray[i++] = component;

        array = newArray;
    }
}
