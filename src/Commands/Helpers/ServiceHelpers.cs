using Commands.Core;
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
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection)
        {
            return collection.ConfigureCommands<CommandManager>(x => { });
        }

        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection,
            Action<ManagerBuilder<CommandManager>> configureDelegate)
        {
            collection.ConfigureCommands<CommandManager>(configureDelegate);

            return collection;
        }

        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection ConfigureCommands<T>(this IServiceCollection collection,
            Action<ManagerBuilder<T>> configureDelegate, params object[] parameters)
            where T : CommandManager
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            var builder = new ManagerBuilder<T>(collection);

            configureDelegate(builder);

            builder.FinalizeConfiguration(parameters);

            return collection;
        }
    }
}
