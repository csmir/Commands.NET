using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Commands;

/// <summary>
///     Provides a set of assertion methods for validating arguments.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Assert
{
    /// <summary>
    ///     Validates that the specified argument is not null.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull(object? argument, string argumentExpression)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, argumentExpression);
#else
        if (argument == null)
            throw new ArgumentNullException(argumentExpression);
#endif
    }

    /// <summary>
    ///     Validates that the specified argument is not null or empty.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrEmpty(string? argument, string argumentExpression)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(argument, argumentExpression);
#else

        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("The argument must not be null or empty.", argumentExpression);
#endif
    }
}
