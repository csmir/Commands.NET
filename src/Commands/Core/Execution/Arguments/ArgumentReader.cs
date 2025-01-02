using System.Text;

namespace Commands;

/// <summary>
///     A thread-safe argument reader, implementing <see cref="string"/> as the raw value.
/// </summary>
/// <remarks>
///     <b>This BPI of this class does not adhere semantic versioning. </b>
///     As edge cases are discovered in the parsing logic, the parsing guidelines may change, and command input might improve or degrade based on different usecases.
/// </remarks>
public static class ArgumentReader
{
    const char U0022 = '"';
    const char U0020 = ' ';
    const char U002D = '-';

#if NET8_0_OR_GREATER

    /// <summary>
    ///     Parses a <see cref="string"/> into a collection of named command arguments.
    /// </summary>
    /// <inheritdoc cref="ReadNamed(string[])"/>
    public static IEnumerable<KeyValuePair<string, object?>> ReadNamed(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return ReadNamed(input.TrimEnd().Split(' '));
    }

    /// <summary>
    ///     Parses a <see cref="string"/> into a collection of command arguments.
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
    /// <param name="input">The string to parse.</param>
    /// <returns>
    ///     A dictionary of key-value pairs representing arguments, where the keys are the argument names. When a pair's value is empty, the name is used instead.
    /// </returns>
    public static IEnumerable<KeyValuePair<string, object?>> ReadNamed(params string[] input)
    {
        if (input is null || input.Length is 0)
            yield break;

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
                        if (argument[1..].Contains(U0022))
                            openState--;
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
                        if (argument[1] is U002D)
                        {
                            if (name is not null)
                                yield return new(name, null);

                            name = argument[2..];

                            continue;
                        }
                    }

                    yield return new(argument[1..], null);

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
#endif

    /// <summary>
    ///     Parses a <see cref="string"/> into a collection of command arguments.
    /// </summary>
    /// <remarks>
    ///     This implementation sets the following guidelines:
    ///     <list type="number">
    ///         <item>
    ///             <b>Whitespace</b> announcements will wrap the previous argument and build a new one.
    ///         </item>
    ///         <item>
    ///             <b>Quotations</b> will wrap the previous argument and build a new one.
    ///         </item>
    ///         <item>
    ///             <b>Quoted</b> arguments will start when a start-quote is discovered, and consider all following whitespace as part of the previous argument. 
    ///             This argument will only be wrapped when an end-quote is announced.
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="toParse">The input string to parse.</param>
    /// <returns>
    ///     A collection of command arguments.
    /// </returns>
    public static IEnumerable<string> Read(string? toParse)
    {
        if (string.IsNullOrWhiteSpace(toParse))
            return [];

        var arr = Array.Empty<string>();
        var sb = new StringBuilder(0, toParse!.Length);
        var quoted = false;

        void Reset()
        {
            if (sb.Length > 0)
            {
                var oLen = arr.Length;
                Array.Resize(ref arr, oLen + 1);

                arr[oLen] = sb.ToString();

                sb.Clear();
            }
        }

        for (int i = 0; i < toParse.Length; i++)
        {
            // If startquote found, skip space check & continue until next occurrence of quote.
            if (quoted)
            {
                if (toParse[i] is U0022)
                {
                    Reset();

                    quoted = false;

                    continue;
                }

                sb.Append(toParse[i]);

                continue;
            }

            if (toParse[i] is U0022)
            {
                if (i + 1 == toParse.Length)
                {
                    break;
                }

                Reset();

                quoted = true;

                continue;
            }

            if (char.IsWhiteSpace(toParse[i]))
            {
                Reset();

                continue;
            }

            // No match for above, add character to current range.
            sb.Append(toParse[i]);
        }

        Reset();

        return arr;
    }
}
