using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     Provides a set of assertion methods for validating arguments.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Assert
    {
        /// <summary>
        ///     Validates that the specified aliases are not null or empty, and that they match the configured naming pattern.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Aliases(IEnumerable<string> aliases, ComponentConfiguration configuration, bool permitNamelessEntry)
        {
            NotNull(aliases, nameof(aliases));

            if (!permitNamelessEntry && !aliases.Any())
                throw new ArgumentException("This nested component must have at least one name.");

            var pattern = configuration.GetProperty<Regex>(ConfigurationPropertyDefinitions.NameValidationExpression);

            foreach (var alias in aliases)
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
        public static void NotNull(object? argument, string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     Validates that the specified argument is not null or empty.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void NotNullOrEmpty(string? argument, string name)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentException("The argument must not be null or empty.", name);
        }
    }
}
