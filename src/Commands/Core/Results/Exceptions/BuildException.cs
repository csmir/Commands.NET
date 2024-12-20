namespace Commands
{
    /// <summary>
    ///     An <see cref="Exception"/> which is thrown when a component fails to be built, modified, or accessed. This class cannot be inherited.
    /// </summary>
    /// <param name="message">The failure message which caused the component to reject an operation.</param>
    /// <param name="innerException">An inner exception that occurred during exception; or null if none occurred.</param>
    public sealed class BuildException(string message, Exception? innerException = null)
        : Exception(message, innerException)
    {
        const string COMPONENT_ALIAS_MISMATCH = $"The component alias '{{0}}' does not match the configured naming pattern.";
        const string COMPONENT_ALIASES_EMPTY = $"The component must have one or more aliases specified at creation.";
        const string COMPONENT_ALIAS_DISTINCT = $"The component alias '{{0}}' already exists on the same component. Components must have only distinctly unique aliases.";
        const string COLLECTION_NOT_SUPPORTED = $"A collection or the element type of said collection is not supported for conversion. Replace, or create a parser to support this type: '{{0}}'";
        const string COMPLEX_NOT_SUPPORTED = $"The type: '{{0}}' must have a public parameterized constructor with at least 1 parameter to be used as a complex argument.";
        const string REMAINDER_NOT_SUPPORTED = $"Remainder must be marked on the last parameter of a command. Command: '{{0}}'";

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
