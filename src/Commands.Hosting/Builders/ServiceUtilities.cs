using Microsoft.Extensions.Hosting;
using System.ComponentModel;

namespace Commands.Builders
{
    /// <summary>
    ///     A utility class that provides extension methods for configuring services with a hosted execution sequence <see cref="ComponentTree"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceUtilities
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureCommands(this IHostBuilder builder, Action<ITreeBuilder> configureAction)
        {
            if (builder.Properties.TryGetValue("Commands:Builder", out var value) && value is ITreeBuilder treeBuilder)
                configureAction(treeBuilder);
            else
            {
                treeBuilder = new ComponentTreeBuilder();
                configureAction(treeBuilder);
                builder.Properties.Add("Commands:Builder", treeBuilder);
            }

            return builder;
        }
    }
}
