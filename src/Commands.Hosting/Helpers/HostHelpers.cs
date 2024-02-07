﻿using Commands.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Helpers
{
    /// <summary>
    ///     A set of helper methods to populate and configure an <see cref="IHostBuilder"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class HostHelpers
    {
        /// <summary>
        ///     Configures the <see cref="IHostBuilder"/> to support use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="BuildingContext"/> with the current <see cref="IHostBuilder"/>'s building context.</param>
        /// <returns>The same <see cref="IHostBuilder"/> for call chaining.</returns>
        public static IHostBuilder ConfigureCommands(this IHostBuilder builder, [DisallowNull] Action<HostBuilderContext, BuildingContext> configureDelegate)
        {
            return builder.ConfigureCommands<CommandManager>(configureDelegate);
        }

        /// <summary>
        ///     Configures the <see cref="IHostBuilder"/> to support use of <typeparamref name="TManager"/> as an implementation of <see cref="CommandManager"/>.
        /// </summary>
        /// <typeparam name="TManager">The implementation of a <see cref="CommandManager"/> to configure for use.</typeparam>
        /// <param name="builder"></param>
        /// <param name="configureDelegate">A delegate to configure the <see cref="BuildingContext"/> with the current <see cref="IHostBuilder"/>'s building context.</param>
        /// <returns>The same <see cref="IHostBuilder"/> for call chaining.</returns>
        public static IHostBuilder ConfigureCommands<TManager>(this IHostBuilder builder, [DisallowNull] Action<HostBuilderContext, BuildingContext> configureDelegate)
            where TManager : CommandManager
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            builder.ConfigureServices((context, services) =>
            {
                var buildContext = new BuildingContext();

                configureDelegate(context, buildContext);

                services.TryAddModules(buildContext);
                services.TryAddSingleton<CommandFinalizer>();

                var descriptor = ServiceDescriptor.Singleton(services =>
                {
                    return ActivatorUtilities.CreateInstance<TManager>(services, buildContext);
                });

                services.TryAdd(descriptor);
            });

            return builder;
        }
    }
}
