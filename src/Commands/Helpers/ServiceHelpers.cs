using Commands.Core;
using Commands.Resolvers;
using Commands.TypeConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Helpers
{
    /// <summary>
    ///     A set of helper methods to populate and configure an <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ServiceHelpers
    {
        #region Converters
        /// <summary>
        ///     Attempts to add a <see cref="TypeConverterBase"/> defined as <typeparamref name="T"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="T">The implementation of <see cref="TypeConverterBase"/> to implement.</typeparam>
        /// <param name="collection"></param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection TryAddConverter<T>(this IServiceCollection collection)
            where T : TypeConverterBase
        {
            var descriptor = ServiceDescriptor.Singleton<TypeConverterBase, T>();

            collection.TryAddEnumerable(descriptor);

            return collection;
        }

        /// <summary>
        ///     Attempts to add a <see cref="TypeConverterBase"/> defined as <paramref name="converter"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="converter">The implementation of <see cref="TypeConverterBase"/> to implement.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection TryAddConverter(this IServiceCollection collection,
            [DisallowNull] TypeConverterBase converter)
        {
            if (converter == null)
            {
                ThrowHelpers.ThrowInvalidArgument(converter);
            }
            var descriptor = ServiceDescriptor.Singleton(converter);

            collection.TryAddEnumerable(descriptor);

            return collection;
        }
        #endregion

        #region Resolvers
        /// <summary>
        ///     Attempts to add a <see cref="ResolverBase"/> defined as <typeparamref name="T"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="T">The implementation of <see cref="ResolverBase"/> to implement.</typeparam>
        /// <param name="collection"></param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection TryAddResolver<T>(this IServiceCollection collection)
            where T : ResolverBase
        {
            var descriptor = ServiceDescriptor.Singleton<ResolverBase, T>();

            collection.TryAddEnumerable(descriptor);
            collection.TryAddSingleton<CommandFinalizer>();

            return collection;
        }

        /// <summary>
        ///     Attempts to add a <see cref="ResolverBase"/> defined as <paramref name="resolver"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="resolver">The implementation of <see cref="ResolverBase"/> to implement.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection TryAddResolver(this IServiceCollection collection,
            [DisallowNull] ResolverBase resolver)
        {
            if (resolver == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resolver);
            }

            var descriptor = ServiceDescriptor.Singleton(resolver);

            collection.TryAddEnumerable(descriptor);

            return collection;
        }

        /// <summary>
        ///     Attempts to add a synchronous post-execution process to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="resultAction">A synchronous result handler to handle post-execution processing.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection TryAddResolver(this IServiceCollection collection,
            [DisallowNull] Action<ConsumerBase, ICommandResult, IServiceProvider> resultAction)
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(ResolverBase), new DelegateResolver(resultAction));

            collection.TryAddEnumerable(descriptor);
            collection.TryAddSingleton<CommandFinalizer>();

            return collection;
        }

        /// <summary>
        ///     Attempts to add an asynchronous post-execution process to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="resultAction"></param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection TryAddResolver(this IServiceCollection collection,
            [DisallowNull] Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resultAction)
        {
            if (resultAction == null)
            {
                ThrowHelpers.ThrowInvalidArgument(resultAction);
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(ResolverBase), new AsyncDelegateResolver(resultAction));

            collection.TryAddEnumerable(descriptor);
            collection.TryAddSingleton<CommandFinalizer>();

            return collection;
        }
        #endregion

        #region Commands
        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> to support use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection)
        {
            return collection.ConfigureCommands<CommandManager>(null);
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> to support use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="contextDelegate">Configures the context responsible for determining the build process.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection ConfigureCommands(this IServiceCollection collection,
            Action<BuildingContext> contextDelegate)
        {
            collection.ConfigureCommands<CommandManager>(contextDelegate);

            return collection;
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> to support use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <typeparam name="T">An implementation of <see cref="CommandManager"/> to replace the default implementation.</typeparam>
        /// <param name="collection"></param>
        /// <param name="contextDelegate">Configures the context responsible for determining the build process.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        public static IServiceCollection ConfigureCommands<T>(this IServiceCollection collection,
            Action<BuildingContext> contextDelegate)
            where T : CommandManager
        {
            var configuration = new BuildingContext();

            contextDelegate?.Invoke(configuration);

            collection.ConfigureCommands<T>(configuration);

            return collection;
        }

        /// <summary>
        ///     Configures the <see cref="IServiceCollection"/> to support use of a <see cref="CommandManager"/>.
        /// </summary>
        /// <typeparam name="T">An implementation of <see cref="CommandManager"/> to replace the default implementation.</typeparam>
        /// <param name="collection"></param>
        /// <param name="context">The context responsible for determining the build process.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IServiceCollection ConfigureCommands<T>(this IServiceCollection collection,
            BuildingContext context)
            where T : CommandManager
        {
            collection.TryAddSingleton<CommandFinalizer>();

            collection.TryAddModules(context);

            var descriptor = ServiceDescriptor.Singleton((services) =>
            {
                return ActivatorUtilities.CreateInstance<T>(services, context);
            });

            collection.TryAdd(descriptor);

            return collection;
        }
        #endregion

        #region Modules
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="context">The context responsible for determining the build process.</param>
        /// <returns>The same <see cref="IServiceCollection"/> for call chaining.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IServiceCollection TryAddModules(this IServiceCollection collection,
            BuildingContext context)
        {
            var rootType = typeof(ModuleBase);

            foreach (var assembly in context.Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (rootType.IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                    {
                        collection.TryAddTransient(type);
                    }
                }
            }

            return collection;
        }
        #endregion
    }
}
