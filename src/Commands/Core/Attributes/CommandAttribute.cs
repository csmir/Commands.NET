using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Commands.Core
{
    /// <summary>
    ///     An attribute that signifies a method as a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        ///     The command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The command's aliases.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Creates a new <see cref="CommandAttribute"/> with provided name.
        /// </summary>
        /// <param name="name">The command name.</param>
        public CommandAttribute([DisallowNull] string name)
            : this(name, [])
        {

        }

        /// <summary>
        ///     Creates a new <see cref="CommandAttribute"/> with provided name and aliases.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="aliases">The command's aliases.</param>
        public CommandAttribute([DisallowNull] string name, params string[] aliases)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ThrowHelpers.ThrowInvalidArgument(name);
            }

            var arr = new string[aliases.Length + 1];
            for (int i = 0; i < aliases.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(aliases[i]))
                {
                    ThrowHelpers.ThrowInvalidArgument(aliases);
                }

                if (arr.Contains(aliases[i]))
                {
                    ThrowHelpers.ThrowNotDistinct(aliases);
                }

                arr[i + 1] = aliases[i];
            }

            if (arr.Contains(name))
            {
                ThrowHelpers.ThrowNotDistinct(aliases);
            }

            arr[0] = name;

            Name = name;
            Aliases = arr;
        }

        internal void ValidateAliases(Regex regex)
        {
            foreach (var alias in Aliases)
            {
                if (!regex.IsMatch(alias))
                {
                    ThrowHelpers.ThrowNotMatched(alias);
                }
            }
        }
    }
}
