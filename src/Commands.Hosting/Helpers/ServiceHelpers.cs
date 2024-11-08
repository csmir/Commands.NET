using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Commands.Helpers
{
    /// <summary>
    ///     A set of helper methods to populate and configure an <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ServiceHelpers
    {
        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> for use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection)
        {
            return collection.ConfigureCommands<CommandManager>(x => { });
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> for use of a <see cref="CommandManager"/> with the provided builder configuration.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="HostCommandBuilder{T}"/> responsible for customizing the <see cref="CommandManager"/> setup.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection,
            Action<HostCommandBuilder<CommandManager>> configureDelegate)
        {
            collection.ConfigureCommands<CommandManager>(configureDelegate);

            return collection;
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> for use of a <see cref="CommandManager"/> with the provided builder configuration.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="ICommandBuilder"/> responsible for customizing the <see cref="CommandManager"/> setup.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
        public static IServiceCollection ConfigureCommands<T>(this IServiceCollection collection,
            Action<HostCommandBuilder<T>> configureDelegate)
            where T : CommandManager
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            var options = new HostCommandBuilder<T>(collection);

            configureDelegate(options);

            var descriptor = ServiceDescriptor.Singleton((services) =>
            {
                return ActivatorUtilities.CreateInstance<T>(services, [options]);
            });

            collection.Add(descriptor);

            return collection;
        }
    }
}
