using Commands.Hosting;

namespace Commands.Http;

/// <summary>
///     A utility class containing methods for working with HTTP hosts in the Commands.NET framework.
/// </summary>
public static class HostUtilities
{
    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureComponents"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<ComponentBuilder> configureComponents)
    {
        return builder;
    }
}
