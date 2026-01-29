using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Commands;

/// <summary>
///     An interface that defines names of a target component, such as a command module, command, or parameter.
/// </summary>
/// <remarks>
///     To use this binding, implement <see cref="NameAttribute"/> directly, or consume another attribute that implements this interface.
/// </remarks>
public interface INameBinding
{
    /// <summary>
    ///     Gets the name of the target.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the names of the target.
    /// </summary>
    /// <remarks>
    ///     Names are not considered for parameter names. Parameters have only one name, defined by <see cref="Name"/>.
    /// </remarks>
    public string[] Names { get; }
}

internal static class NameBindingValidation
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrInvalid(IEnumerable<string> values, Regex? regex, string argumentExpression)
    {
        if (values == null)
            throw new ArgumentNullException(argumentExpression);

        if (regex == null) return;

        if (!values.Any())
            throw new ArgumentException($"The argument '{argumentExpression}' must not be empty.", argumentExpression);

        foreach (var value in values)
        {
            if (value == null)
                throw new ArgumentNullException(argumentExpression);

            if (!regex.IsMatch(value))
                throw new ArgumentException($"The argument '{argumentExpression}' must match the validation expression '{regex}'", argumentExpression);
        }
    }
}
