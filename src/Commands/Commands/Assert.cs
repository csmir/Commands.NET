﻿using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Commands;

internal static class Assert
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull(object? argument, string argumentExpression)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, argumentExpression);
#else
        if (argument == null)
            throw new ArgumentNullException(argumentExpression);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrEmpty(string? argument, string argumentExpression)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(argument, argumentExpression);
#else
        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("The argument must not be null or empty.", argumentExpression);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrInvalid(IEnumerable<string> values, Regex? regex, string argumentExpression)
    {
        NotNull(values, nameof(values));

        if (regex != null)
        {
            var valueCount = 0;
            foreach (var value in values)
            {
                valueCount++;

                NotNullOrEmpty(value, argumentExpression);

                if (!regex.IsMatch(value))
                    throw new ArgumentException($"The argument '{argumentExpression}' must match the validation expression '{regex}'", argumentExpression);
            }

            if (valueCount == 0)
                throw new ArgumentException($"The argument '{argumentExpression}' must not be empty.", argumentExpression);
        }
    }
}
