namespace Commands;

/// <summary>
///     Represents a mechanism for enumerating arguments while searching or parsing a command.
/// </summary>
public struct ArgumentDictionary
{
#if NET8_0_OR_GREATER
    const char U0022 = '"';
    const char U0020 = ' ';
    const char U002D = '-';
#else
    const string U0022 = "\"";
    const string U0020 = " ";
    const string U002D = "-";
#endif

    private int _index = 0;

    private readonly List<string> _unnamedArgs;
    private readonly Dictionary<string, object?> _namedArgs;

    internal int AvailableLength { get; private set; }

    /// <summary>
    ///     Gets the number of arguments present in the set.
    /// </summary>
    public readonly int Count
        => _unnamedArgs.Count + _namedArgs.Count;

    /// <summary>
    ///     Gets a key-value pair from the set of arguments, known by the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key under which this argument is known to the current array.</param>
    /// <returns>A </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the provided <paramref name="key"/> is not found in the set.</exception>
    public readonly KeyValuePair<string, object?> this[string key]
    {
        get
        {
            if (_namedArgs.TryGetValue(key, out var value))
                return new(key, value);

            if (_unnamedArgs.Contains(key, StringComparer.OrdinalIgnoreCase))
                return new(key, null);

            throw new KeyNotFoundException();
        }
    }

    /// <summary>
    ///     Creates a new <see cref="ArgumentDictionary"/> from a set of named arguments.
    /// </summary>
    /// <param name="args">The range of named arguments to enumerate in this set.</param>
    /// <param name="comparer">The comparer to evaluate keys in the inner named dictionary.</param>
    public ArgumentDictionary(StringComparer? comparer, IEnumerable<KeyValuePair<string, object?>> args)
    {
        _namedArgs = new(comparer);

        var unnamedFill = new List<string>();

        foreach (var kvp in args)
        {
            if (kvp.Value == null)
                unnamedFill.Add(kvp.Key);
            else
                _namedArgs[kvp.Key] = kvp.Value;
        }

        _unnamedArgs = unnamedFill;
        AvailableLength = _unnamedArgs.Count + _namedArgs.Count;
    }

    /// <summary>
    ///     Creates a new empty <see cref="ArgumentDictionary"/>.
    /// </summary>
    public ArgumentDictionary()
    {
        _namedArgs = null!;
        _unnamedArgs = null!;
        AvailableLength = 0;
    }

    #region Internals

#if NET8_0_OR_GREATER
    internal bool TryGetValue(string parameterName, [NotNullWhen(true)] out object? value)
#else
    internal bool TryGetValue(string parameterName, out object? value)
#endif
    {
        if (_namedArgs.TryGetValue(parameterName, out value!))
            return true;

        if (_index >= _unnamedArgs.Count)
            return false;

        value = _unnamedArgs[_index++];

        return true;
    }

#if NET8_0_OR_GREATER
    internal readonly bool TryGetElementAt(int index, [NotNullWhen(true)] out string? value)
#else
    internal readonly bool TryGetElementAt(int index, out string? value)
#endif
    {
        if (index < _unnamedArgs.Count)
        {
            value = _unnamedArgs[index];
            return true;
        }

        value = null;
        return false;
    }

    internal readonly string TakeRemaining(string parameterName, char separator)
#if NET8_0_OR_GREATER
        => string.Join(separator, TakeRemaining(parameterName));
#else
        => string.Join($"{separator}", TakeRemaining(parameterName));
#endif

    internal readonly IEnumerable<object> TakeRemaining(string parameterName)
    {
        if (_namedArgs.TryGetValue(parameterName, out var value))
            return [value!, .. _unnamedArgs.Skip(_index)];

        return _unnamedArgs.Skip(_index);
    }

    internal void SetParseIndex(int index)
    {
        _index = index;
        AvailableLength -= index;
    }

    #endregion

    #region Initializers

    /// <inheritdoc cref="From(string, char[], StringComparer?)"/>
    public static ArgumentDictionary From(string? input, StringComparer? comparer = null)
        => From(input, [' '], comparer);

    /// <inheritdoc cref="From(string, char[], StringComparer?)"/>
    public static ArgumentDictionary From(string[] input, StringComparer? comparer = null)
    {
        if (input.Length == 0)
            return new();

        return new(comparer, ReadInternal(input));
    }

    /// <summary>
    ///     Reads the provided <paramref name="input"/> into an array of command arguments. This method will never throw, always returning a new <see cref="ArgumentDictionary"/>.
    /// </summary>
    /// <remarks>
    ///     The implementation is defined by the following rules:
    ///     <list type="number">
    ///         <item>
    ///             <b>Flags</b> can act either as a standalone argument when prefixed with one hyphen <c>-</c>, or as a name for the next item, assuming a prefix of two hyphens <c>--</c>, and that the next item is not also a flag.
    ///         </item>
    ///         <item>
    ///             <b>Whitespace</b> acts as a delimiter. When preceded by an argument name, it treats the next item as its value, otherwise closing a pair.
    ///         </item>
    ///         <item>
    ///             <b>Quotations</b> start concatenation when at the start of an argument, where all following arguments are collected until an end-quote is found.<br />
    ///             <i>Note: A quote is only considered an end-quote if it is the lowest level quote in all following arguments.</i>
    ///         </item>
    ///         <item>
    ///             <b>Unnamed</b> arguments are added to the collection as a key with a <see langword="null"/> value.
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="input">The caller input to parse into a set of arguments.</param>
    /// <param name="separators">The characters to use as separators when splitting the input.</param>
    /// <param name="comparer">The comparer to use when comparing argument names.</param>
    /// <returns>
    ///     An array of arguments that can be used to search for a command or parse into a method.
    /// </returns>
    public static ArgumentDictionary From(string? input, char[] separators, StringComparer? comparer = null)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new();

        var split = input!.Split(separators);

        if (split.Length == 0)
            return new();

        return new(comparer, ReadInternal(split));
    }

    private static IEnumerable<KeyValuePair<string, object?>> ReadInternal(string[] input)
    {
        if (input.Length is 1)
        {
            yield return new(input[0], null);
            yield break;
        }

        // Reserved for joining arguments.
        var openState = 0;
        var concatenating = false;
        var concatenation = new List<string>();

        // Reserved for named arguments.
        string? name = null;

        foreach (var argument in input)
        {
            if (concatenating)
            {
                if (argument.StartsWith(U0022))
                {
                    openState++;

                    concatenation.Add(argument);

                    if (argument.Length > 1)
                    {
#if NET8_0_OR_GREATER
                        if (argument[1..].Contains(U0022))
                            openState--;
#else
                        if (argument.Remove(0, 1).Contains(U0022))
                            openState--;
#endif
                    }

                    continue;
                }

                if (argument.EndsWith(U0022))
                {
                    if (openState is 0)
                    {
                        concatenating = false;

                        concatenation.Add(argument);

                        if (name is null)
                            yield return new(string.Join(U0020, concatenation), null);
                        else
                        {
                            yield return new(name, string.Join(U0020, concatenation));

                            name = null;
                        }

                        concatenation.Clear();
                    }
                    else
                    {
                        openState--;

                        concatenation.Add(argument);
                    }

                    continue;
                }

                concatenation.Add(argument);
            }
            else
            {
                if (argument.StartsWith(U002D))
                {
                    if (argument.Length > 1)
                    {
#if NET8_0_OR_GREATER
                        if (argument[1] is U002D)
                        {
                            if (name is not null)
                                yield return new(name, null);

                            name = argument[2..];

                            continue;
                        }
                    }

                    yield return new(argument[1..], null);
#else
                        if (argument[1] == U002D[0])
                        {
                            if (name is not null)
                                yield return new(name, null);

                            name = argument.Remove(0, 2);

                            continue;
                        }
                    }

                    yield return new(argument.Remove(0, 1), null);
#endif

                    continue;
                }

                if (argument.StartsWith(U0022) && !argument.EndsWith(U0022))
                {
                    concatenating = true;

                    concatenation.Add(argument);

                    continue;
                }

                if (name is null)
                    yield return new(argument, null);
                else
                {
                    yield return new(name, argument);

                    name = null;
                }
            }
        }

        // If concatenation is still filled on escaping the sequence, add as last argument.
        if (concatenation.Count != 0)
        {
            if (name is null)
                yield return new(string.Join(U0020, concatenation), null);
            else
                yield return new(name, string.Join(U0020, concatenation));
        }
    }

    #endregion
}
