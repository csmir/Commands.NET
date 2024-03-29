﻿using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     An attribute that defines the name of a module (<see cref="ModuleBase"/>), a module's members, a command and a command parameter.
    /// </summary>
    /// <remarks>
    ///     This attribute defines the name of a top-level component as well as all its members. 
    ///     If a <see cref="ModuleBase"/> is named and its invokable members (command methods) are not, they will take on the name of the module instead, serving as default overloads.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public sealed class NameAttribute : Attribute
    {
        /// <summary>
        ///     Gets the name of the target.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the aliases of the target.
        /// </summary>
        /// <remarks>
        ///     Aliases are not considered for parameter names. Parameters have only one name, defined by <see cref="Name"/>.
        /// </remarks>
        public string[] Aliases { get; }

        /// <summary>
        ///     Creates a new <see cref="NameAttribute"/> with defined name.
        /// </summary>
        /// <param name="name">The target name.</param>
        public NameAttribute([DisallowNull] string name)
            : this(name, [])
        {

        }

        /// <summary>
        ///     Creates a new <see cref="NameAttribute"/> with defined name and aliases.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="aliases">The target's aliases. Aliases are not considered for parameter names.</param>
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
