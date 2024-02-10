using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Commands.Core
{
    /// <summary>
    ///     An attribute that signifies a module to be a group, allowing functionality much like subcommands.
    /// </summary>
    /// <remarks>
    ///     This attribute does not work on top-level modules.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class GroupAttribute : Attribute
    {
        /// <summary>
        ///     Represents the name of a group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The aliases of this group.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Creates a new <see cref="GroupAttribute"/> with defined name.
        /// </summary>
        /// <param name="name">The group name.</param>
        public GroupAttribute([DisallowNull] string name)
            : this(name, [])
        {

        }

        /// <summary>
        ///     Creates a new <see cref="GroupAttribute"/> with defined name.
        /// </summary>
        /// <param name="name">The group name.</param>
        /// <param name="aliases">The group's aliases.</param>
        public GroupAttribute([DisallowNull] string name, params string[] aliases)
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
