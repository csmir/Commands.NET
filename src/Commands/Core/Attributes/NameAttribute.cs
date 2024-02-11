using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     An attribute that defines the name of a module (<see cref="ModuleBase"/>), a module subtype, a 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public sealed class NameAttribute : Attribute
    {
        /// <summary>
        ///     Represents the name of this component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The aliases of this component.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Creates a new <see cref="NameAttribute"/> with defined name.
        /// </summary>
        /// <param name="name">The component name.</param>
        public NameAttribute([DisallowNull] string name)
            : this(name, [])
        {

        }

        /// <summary>
        ///     Creates a new <see cref="NameAttribute"/> with defined name.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="aliases">The component's aliases.</param>
        public NameAttribute([DisallowNull] string name, params string[] aliases)
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
