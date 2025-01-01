using System.Text.RegularExpressions;

namespace Commands
{
    /// <summary>
    ///     An attribute that defines the name of a module (<see cref="CommandModule"/>), a declared command or a command parameter. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This attribute defines the name of a top-level component as well as all its members. 
    ///     If a <see cref="CommandModule"/> is named and its invokable members (command methods) are not, they will take on the name of the module instead, serving as default overloads.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class NameAttribute : Attribute
    {
        /// <summary>
        ///     Gets the name of the target.
        /// </summary>
        public string Name 
            => Aliases[0];

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
        public NameAttribute(string name)
            : this([name])
        {

        }

        /// <summary>
        ///     Creates a new <see cref="NameAttribute"/> with defined name and aliases.
        /// </summary>
        /// <param name="aliases">The target's aliases. Aliases are not considered for parameter names.</param>
        public NameAttribute(params string[] aliases)
        {
            if (aliases.Length == 0)
                throw new ArgumentException("At least one alias must be provided.", nameof(aliases));

            Aliases = aliases;
        }
    }
}
