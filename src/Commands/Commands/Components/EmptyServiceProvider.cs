namespace Commands;

internal sealed class EmptyServiceProvider : IServiceProvider
{
    public static EmptyServiceProvider Instance { get; } = new();

    public object? GetService(Type serviceType)
        => null;
}