using System.Text.RegularExpressions;

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
        const string SEALED_COMPONENT_ACCESS_ERROR = "Cannot access a sealed component.";
        const string COMPONENT_ALIAS_MISMATCH = "The component alias '{0}' does not match the provided naming pattern in the BuildConfiguration.";
        const string COMPONENT_ALIASES_EMPTY = "The component must have one or more aliases specified at creation. Consider using NameAttribute for declared signatures or provide aliases to the XBuilder used to create this component.";
        const string COLLECTION_NOT_SUPPORTED = "A collection or the element type of said collection is not supported for conversion. Add a TypeConverter to the BuildConfigurationBuilder or CommandTreeConfiguration to support this type: '{0}'";
        const string COMPLEX_NOT_SUPPORTED = "The type: '{0}' must have a public parameterized constructor with at least 1 parameter to be used as a complex argument marked by ComplexAttribute.";

        internal static BuildException AccessDenied()
            => new(SEALED_COMPONENT_ACCESS_ERROR);

        internal static BuildException AliasMismatch(string alias)
            => new(string.Format(COMPONENT_ALIAS_MISMATCH, alias));

        internal static BuildException AliasAtLeastOne()
            => new(COMPONENT_ALIASES_EMPTY);

        internal static BuildException CollectionNotSupported(Type type)
            => new(string.Format(COLLECTION_NOT_SUPPORTED, type.Name));

        internal static BuildException ComplexNotSupported(Type type)
            => new(string.Format(COMPLEX_NOT_SUPPORTED, type.Name));
    }
}
