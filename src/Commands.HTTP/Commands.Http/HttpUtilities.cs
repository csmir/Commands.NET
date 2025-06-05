using Commands.Hosting;

namespace Commands.Http;

/// <summary>
///     A utility class containing methods for working with HTTP hosts in the Commands.NET framework.
/// </summary>
public static class HttpUtilities
{
    /// <inheritdoc cref="HostUtilities.ConfigureComponents(IHostBuilder, Action{ComponentBuilder})"/>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<HttpComponentBuilder> configureComponents)
        => ConfigureHttpComponents(builder, (context, httpBuilder) => configureComponents(httpBuilder));

    /// <inheritdoc cref="HostUtilities.ConfigureComponents(IHostBuilder, Action{ComponentBuilder})"/>
    public static IHostBuilder ConfigureHttpComponents(this IHostBuilder builder, Action<HostBuilderContext, HttpComponentBuilder> configureComponents)
    {
        Assert.NotNull(configureComponents, nameof(configureComponents));

        var httpBuilder = new HttpComponentBuilder();

        builder.ConfigureServices((context, services) => configureComponents(context, httpBuilder));

        return builder.ConfigureComponents(httpBuilder);
    }
}
