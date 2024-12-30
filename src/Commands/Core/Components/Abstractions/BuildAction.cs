namespace Commands
{
    /// <summary>
    ///     A collection of actions that can occur during the build process, from creating a new component instance to configuring its parent or child components.
    /// </summary>
    public enum BuildAction
    {
        ModuleDiscovery,

        ModuleDiscovered,

        CommandDiscovery,

        CommandDiscovered,

        ComponentCreating,

        ComponentCreated,

        ComponentSkipped,

        ArgumentsDiscovery,

        ArgumentsDiscovered,

        ParserDiscovery,

        ParserDiscovered,
    }
}
