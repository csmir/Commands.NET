using Commands.Conditions;
using Commands.Helpers;
using Commands.TypeConverters;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command module, hosting zero-or-more commands.
    /// </summary>
    public sealed class ModuleInfo : IConditional
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public PreconditionAttribute[] Preconditions { get; }

        /// <inheritdoc />
        public PostconditionAttribute[] PostConditions { get; }

        /// <summary>
        ///     Gets an array containing nested modules or commands inside this module.
        /// </summary>
        public IConditional[] Components { get; }

        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the root module.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if this module is not nested.
        /// </remarks>
        public ModuleInfo Root { get; }

        internal ModuleInfo(
            Type type, IDictionary<Type, TypeConverterBase> converters, ModuleInfo root = null, string expectedName = null, string[] aliases = null)
        {
            var attributes = type.GetAttributes(true);
            var preconditions = attributes.GetPreconditions();
            var postconditions = attributes.GetPostconditions();

            Root = root;
            Type = type;

            Attributes = attributes;
            Preconditions = preconditions;
            PostConditions = postconditions;

            Components = this.GetComponents(converters);

            Name = expectedName ?? type.Name;
            Aliases = aliases ?? [Name];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{(Root != null ? $"{Root}." : "")}{(Type.Name != Name ? $"{Type.Name}['{Name}']" : $"{Name}")}";
        }
    }
}
