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
    ///     Validates that the specified names are not null or empty, and that they match the configured naming pattern.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Names(IEnumerable<string> names, ComponentConfiguration configuration, bool allowUnnamed)
    {
        NotNull(names, nameof(names));

        if (!allowUnnamed && !names.Any())
            throw new ArgumentException("Nested or unbound components must have at least one name.");

        var pattern = configuration.GetProperty<Regex>("NameValidationExpression");

        foreach (var alias in names)
        {
            NotNullOrEmpty(alias, nameof(alias));

            if (pattern?.IsMatch(alias) == false)
                throw new ArgumentException($"The component alias '{alias}' does not match the configured naming pattern.");
        }
    }

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
