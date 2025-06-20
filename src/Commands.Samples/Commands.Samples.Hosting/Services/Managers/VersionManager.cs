namespace Commands.Samples;

// A singleton class that manages the version of the application. This class has no access to scoped functionality, but allowing it to hold values for the application lifetime.
public sealed class VersionManager
{
    public Version CurrentVersion { get; set; } = new Version(1, 0, 0, 0);
}
