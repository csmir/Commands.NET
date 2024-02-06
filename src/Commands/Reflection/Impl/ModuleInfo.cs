using Commands.Helpers;
using Commands.Preconditions;
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
        public bool HasPreconditions { get; }

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

        internal ModuleInfo(Type type, IDictionary<Type, TypeConverter> typeReaders, ModuleInfo root = null, string expectedName = null, string[] aliases = null)
        {
            var attributes = type.GetAttributes(true);
            var preconditions = attributes.GetPreconditions();

            Root = root;
            Type = type;

            Attributes = attributes;
            Preconditions = preconditions;
            HasPreconditions = preconditions.Length > 0;

            Components = this.GetComponents(typeReaders);

            Name = expectedName ?? type.Name;
            Aliases = aliases ?? [Name];
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{(Root != null ? $"{Root}." : "")}{(Type.Name != Name ? $"{Type.Name}['{Name}']" : $"{Name}")}";
    }
}
