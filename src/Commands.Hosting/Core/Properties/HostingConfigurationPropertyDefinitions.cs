using System.ComponentModel;

namespace Commands;

/// <summary>
///     Defines the names of the configuration properties that are used to configure a hosted Commands.NET environment.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class HostingConfigurationPropertyDefinitions
{
    /// <summary>
    ///     Gets the name of the configuration property that defines the collection of source resolvers that are used to resolve command input.
    /// </summary>
    public const string SourceResolverCollection = "SourceResolverCollection";
}
