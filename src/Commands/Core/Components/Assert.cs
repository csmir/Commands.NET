using System.ComponentModel;
using System.Text.RegularExpressions;

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
    public static void NotNull(object? argument, string argumentExpression)
    {
        if (argument == null)
            throw new ArgumentNullException(argumentExpression);
    }

    /// <summary>
    ///     Validates that the specified argument is not null or empty.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void NotNullOrEmpty(string? argument, string argumentExpression)
    {
        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("The argument must not be null or empty.", argumentExpression);
    }
}
