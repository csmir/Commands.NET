using Commands.Converters;
using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     A <see cref="CommandException"/> which is thrown when a component fails to be built, modified, or accessed.
    /// </summary>
    /// <param name="message">The failure message which caused the component to reject an operation.</param>
    /// <param name="innerException">An inner exception that occurred during exception; or null if none occurred.</param>
    public class BuildException(string message, Exception? innerException = null)
        : CommandException(message, innerException)
    {
        // Runtime access to sealed component collection
        const string SEALED_COMPONENT_ACCESS_ERROR = $"The {nameof(ComponentCollection)} has been marked read-only at creation and cannot be mutated.";
        
        // Component creation errors
        const string COMPONENT_ALIAS_MISMATCH = $"The component alias '{{0}}' does not match the provided naming pattern in the {nameof(BuildConfiguration)}.";
        const string COMPONENT_ALIASES_EMPTY  = $"The component must have one or more aliases specified at creation. Consider using {nameof(NameAttribute)} for declared signatures or provide aliases to the {nameof(IComponentBuilder)} used to create this component.";
        const string COMPONENT_ALIAS_DISTINCT = $"The component alias '{{0}}' already exists on the same component. Components must have only distinctly unique aliases.";
        const string COLLECTION_NOT_SUPPORTED = $"A collection or the element type of said collection is not supported for conversion. Add a {nameof(TypeConverter)} to the {nameof(CommandTreeBuilder)} or {nameof(BuildConfigurationBuilder)} to support this type: '{{0}}'";
        const string COMPLEX_NOT_SUPPORTED    = $"The type: '{{0}}' must have a public parameterized constructor with at least 1 parameter to be used as a complex argument marked by {nameof(ComplexAttribute)}.";
        const string REMAINDER_NOT_SUPPORTED  = $"{nameof(RemainderAttribute)} must be marked on the last parameter of a command. Command: '{{0}}'";

        internal static BuildException AccessDenied()
            => new(SEALED_COMPONENT_ACCESS_ERROR);

        internal static BuildException AliasConvention(string alias)
            => new(string.Format(COMPONENT_ALIAS_MISMATCH, alias));

        internal static BuildException AliasAtLeastOne()
            => new(COMPONENT_ALIASES_EMPTY);

        internal static BuildException AliasDistinct(string alias)
            => new(string.Format(COMPONENT_ALIAS_DISTINCT, alias));

        internal static BuildException CollectionNotSupported(Type type)
            => new(string.Format(COLLECTION_NOT_SUPPORTED, type.Name));

        internal static BuildException ComplexNotSupported(Type type)
            => new(string.Format(COMPLEX_NOT_SUPPORTED, type.Name));

        internal static BuildException RemainderNotSupported(string name)
            => new(string.Format(REMAINDER_NOT_SUPPORTED, name));
    }
}
