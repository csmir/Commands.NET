using Commands.Converters;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace Commands
{
    /// <summary>
    ///     A utility class that provides extension methods for configuring services with a hosted execution sequence <see cref="CommandManager"/>.
    /// </summary>
    public static class ServiceUtilities
    {
        /// <summary>
        ///     Tries to add a default <see cref="SourceResolverBase"/> to the service collection. If a resolver is already added, this add operation will be skipped.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddSourceResolver(this IHostBuilder builder)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<SourceResolverBase, DefaultSourceResolver>();
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="SourceResolverBase"/> to the service collection by the specified type.  If a resolver of the same type is already added, this add operation will be skipped.
        /// </summary>
        /// <typeparam name="TResolver">The type of the resolver that should be added to the service collection.</typeparam>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddSourceResolver<TResolver>(this IHostBuilder builder)
            where TResolver : SourceResolverBase
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<SourceResolverBase, TResolver>();
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="SourceResolverBase"/> to the service collection by the specified delegate. If a delegate resolver is already added, this add operation will be skipped.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="resolveDelegate">The delegate is invoked runs when the input of a command is requested.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddSourceResolver(this IHostBuilder builder, Func<SourceResult> resolveDelegate)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<SourceResolverBase>(new DelegateSourceResolver(resolveDelegate));
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="SourceResolverBase"/> to the service collection by the specified delegate. If a delegate resolver is already added, this add operation will be skipped.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="resolveDelegate">The delegate is invoked runs when the input of a command is requested.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddSourceResolver(this IHostBuilder builder, Func<ValueTask<SourceResult>> resolveDelegate)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<SourceResolverBase>(new AsyncDelegateSourceResolver(resolveDelegate));
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a default <see cref="ResultResolverBase"/> to the service collection. If a resolver is already added, this add operation will be skipped.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddResultResolver(this IHostBuilder builder)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                services.TryAddSingleton<ResultResolverBase, DefaultResultResolver>();
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="ResultResolverBase"/> to the service collection by the specified type. If a resolver with the same type is already added, this add operation will be skipped.
        /// </summary>
        /// <typeparam name="TResolver">The type of the resolver that should be added to the service collection.</typeparam>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="scopeToExecution">Determines if the service should be scoped to the command execution. If <see langword="false"/>, the service will be added as a singleton.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddResultResolver<TResolver>(this IHostBuilder builder, bool scopeToExecution = false)
            where TResolver : ResultResolverBase
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (scopeToExecution)
                    services.TryAddScoped<ResultResolverBase, TResolver>();
                else
                    services.TryAddSingleton<ResultResolverBase, TResolver>();
            });

            return builder;
        }

        /// <summary>
        ///     Try to add a <see cref="ResultResolverBase"/> to the service collection by the specified delegate. If a delegate resolver is already added, this add operation will be skipped.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="resolveDelegate">The delegate that is invoked when the result of a command needs to be handled.</param>
        /// <param name="scopeToExecution">Determines if the service should be scoped to the command execution. If <see langword="false"/>, the service will be added as a singleton.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddResultResolver(this IHostBuilder builder, Action<ConsumerBase, ICommandResult, IServiceProvider> resolveDelegate, bool scopeToExecution = false)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (scopeToExecution)
                    services.TryAddScoped(typeof(ResultResolverBase), (services) => new DelegateResolver(resolveDelegate));
                else
                    services.TryAddSingleton<ResultResolverBase>(new DelegateResolver(resolveDelegate));
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="ResultResolverBase"/> to the service collection by the specified delegate. If a delegate resolver is already added, this add operation will be skipped.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="resolveDelegate">The delegate that is invoked when the result of a command needs to be handled.</param>
        /// <param name="scopeToExecution">Determines if the service should be scoped to the command execution. If <see langword="false"/>, the service will be added as a singleton.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddResultResolver(this IHostBuilder builder, Func<ConsumerBase, ICommandResult, IServiceProvider, ValueTask> resolveDelegate, bool scopeToExecution = false)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (scopeToExecution)
                    services.TryAddScoped(typeof(ResultResolverBase), (services) => new AsyncDelegateResolver(resolveDelegate));
                else
                    services.TryAddSingleton<ResultResolverBase>(new AsyncDelegateResolver(resolveDelegate));
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="TypeConverterBase"/> to the service collection by the specified type. If a converter of the same type is already added, this add operation will be skipped.
        /// </summary>
        /// <typeparam name="TConverter"></typeparam>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="scopeToExecution">Determines if the service should be scoped to the command execution. If <see langword="false"/>, the service will be added as a singleton.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddTypeConverter<TConverter>(this IHostBuilder builder, bool scopeToExecution = false)
            where TConverter : TypeConverterBase
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (scopeToExecution)
                    services.TryAddScoped<TypeConverterBase, TConverter>();
                else
                    services.TryAddSingleton<TypeConverterBase, TConverter>();
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="TypeConverterBase"/> to the service collection by the specified delegate. If a converter with the same conversion type is already added, this add operation will be skipped.
        /// </summary>
        /// <typeparam name="TConvertible"></typeparam>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="convertDelegate">The delegate that is invoked when the conversion of a command argument of the given type is requested.</param>
        /// <param name="scopeToExecution">Determines if the service should be scoped to the command execution. If <see langword="false"/>, the service will be added as a singleton.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddTypeConverter<TConvertible>(this IHostBuilder builder, Func<ConsumerBase, IArgument, string?, IServiceProvider, ConvertResult> convertDelegate, bool scopeToExecution = false)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (scopeToExecution)
                {
                    var descriptor = ServiceDescriptor.Scoped<TypeConverterBase, DelegateConverter<TConvertible>>((services) => new DelegateConverter<TConvertible>(convertDelegate));

                    services.TryAddEnumerable(descriptor);
                }
                else
                {
                    var descriptor = ServiceDescriptor.Singleton<TypeConverterBase, DelegateConverter<TConvertible>>((services) => new DelegateConverter<TConvertible>(convertDelegate));

                    services.TryAddEnumerable(descriptor);
                }
            });

            return builder;
        }

        /// <summary>
        ///     Tries to add a <see cref="TypeConverterBase"/> to the service collection by the specified delegate. If a converter with the same conversion type is already added, this add operation will be skipped.
        /// </summary>
        /// <typeparam name="TConvertible"></typeparam>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="convertDelegate">The delegate that is invoked when the conversion of a command argument of the given type is requested.</param>
        /// <param name="scopeToExecution">Determines if the service should be scoped to the command execution. If <see langword="false"/>, the service will be added as a singleton.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddTypeConverter<TConvertible>(this IHostBuilder builder, Func<ConsumerBase, IArgument, string?, IServiceProvider, ValueTask<ConvertResult>> convertDelegate, bool scopeToExecution = false)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (scopeToExecution)
                {
                    var descriptor = ServiceDescriptor.Scoped<TypeConverterBase, AsyncDelegateConverter<TConvertible>>((services) => new AsyncDelegateConverter<TConvertible>(convertDelegate));

                    services.TryAddEnumerable(descriptor);
                }
                else
                {
                    var descriptor = ServiceDescriptor.Singleton<TypeConverterBase, AsyncDelegateConverter<TConvertible>>((services) => new AsyncDelegateConverter<TConvertible>(convertDelegate));

                    services.TryAddEnumerable(descriptor);
                }
            });

            return builder;
        }

        /// <summary>
        ///     Adds a command to the properties collection of the host context with the specified name and delegate.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="name">The name of the command, which will be used to search it up to be executed.</param>
        /// <param name="executeDelegate">The delegate responsible for executing the command.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddCommand(this IHostBuilder builder, string name, Delegate executeDelegate)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (context.Properties.TryGetValue("Commands:Commands", out var propCmd))
                {
                    if (propCmd is List<CommandBase> commands)
                    {
                        commands.Add(new(name, [], executeDelegate));
                    }
                }
                else
                {
                    context.Properties.Add("Commands:Commands", new List<CommandBase> { new(name, [], executeDelegate) });
                }
            });
            return builder;
        }

        /// <summary>
        ///     Adds a command to the properties collection of the host context with the specified name, delegate, and aliases.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="name">The name of the command, which will be used to search it up to be executed.</param>
        /// <param name="executeDelegate">The delegate responsible for executing the command.</param>
        /// <param name="aliases">All additional aliases by which the command can be requested.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddCommand(this IHostBuilder builder, string name, Delegate executeDelegate, params string[] aliases)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (context.Properties.TryGetValue("Commands:Commands", out var propCmd))
                {
                    if (propCmd is List<CommandBase> commands)
                    {
                        commands.Add(new(name, aliases, executeDelegate));
                    }
                }
                else
                {
                    context.Properties.Add("Commands:Commands", new List<CommandBase> { new(name, aliases, executeDelegate) });
                }
            });
            return builder;
        }

        /// <summary>
        ///     Adds a command to the properties collection of the host context with the specified names and delegate.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="names">An array of names for this command. This array expects at least one value.</param>
        /// <param name="executeDelegate">The delegate responsible for executing the command.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when the array of names of the command is empty.</exception>
        public static IHostBuilder AddCommand(this IHostBuilder builder, string[] names, Delegate executeDelegate)
        {
            EnsureConfigured(builder);

            if (names.Length == 0)
            {
                throw new ArgumentException("At least one name must be provided.", nameof(names));
            }

            var aliases = names.Length > 1 ? names[1..] : [];

            builder.ConfigureServices((context, services) =>
            {
                if (context.Properties.TryGetValue("Commands:Commands", out var propCmd))
                {
                    if (propCmd is List<CommandBase> range)
                    {
                        range.Add(new(names[0], aliases, executeDelegate));
                    }
                }
                else
                {
                    context.Properties.Add("Commands:Commands", new List<CommandBase> { new(names[0], aliases, executeDelegate) });
                }
            });

            return builder;
        }

        /// <summary>
        ///     Adds an assembly to the properties collection of the host context to be scanned for commands.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <param name="assembly">The assembly that should be scanned for commands.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddCommandAssembly(this IHostBuilder builder, Assembly assembly)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (context.Properties.TryGetValue("Commands:Assemblies", out var propAsm))
                {
                    if (propAsm is List<Assembly> assemblies)
                    {
                        assemblies.Add(assembly);
                    }
                }
                else
                {
                    context.Properties.Add("Commands:Assemblies", new List<Assembly> { assembly });
                }
            });

            return builder;
        }

        /// <summary>
        ///     Adds the calling assembly to the properties collection of the host context to be scanned for commands.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder AddCommandAssembly(this IHostBuilder builder)
        {
            EnsureConfigured(builder);

            builder.ConfigureServices((context, services) =>
            {
                if (context.Properties.TryGetValue("Commands:Assemblies", out var propAsm))
                {
                    if (propAsm is List<Assembly> assemblies)
                    {
                        assemblies.Add(Assembly.GetCallingAssembly());
                    }
                }
                else
                {
                    context.Properties.Add("Commands:Assemblies", new List<Assembly> { Assembly.GetCallingAssembly() });
                }
            });

            return builder;
        }

        /// <summary>
        ///     Configures the host builder to use the commands execution sequence. This method is used to add all necessary services to the service collection.
        /// </summary>
        /// <param name="builder">The builder that configures the underlying <see cref="IServiceProvider"/>.</param>
        /// <returns>The same instance of <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder UseCommands(this IHostBuilder builder)
        {
            // These inner calls call EnsureConfigured, it is not necessary to call explicitly.

            // Adds default resolvers defined in the Commands.Resolvers namespace.
            builder.AddResultResolver();
            builder.AddSourceResolver();

            return builder;
        }

        private static void EnsureConfigured(IHostBuilder builder)
        {
            if (builder.Properties.ContainsKey("Commands:Configured"))
            {
                return;
            }

            builder.Properties.Add("Commands:Configured", true);

            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<SequenceInitiator>();
                services.AddSingleton<ConfigurationBuilder>();

                var descriptor = ServiceDescriptor.Singleton((services) =>
                {
                    var builder = services.GetRequiredService<ConfigurationBuilder>();

                    var resolvers = services.GetServices<ResultResolverBase>();
                    var converters = services.GetServices<TypeConverterBase>();

                    var assemblies = context.Properties.TryGetValue("Commands:Assemblies", out var propAsm)
                        ? propAsm as List<Assembly>
                        : [];

                    var commands = context.Properties.TryGetValue("Commands:Commands", out var propCmd)
                        ? propCmd as List<CommandBase>
                        : [];

                    foreach (var converter in converters)
                    {
                        builder.TypeConverters.Add(converter.Type, converter);
                    }

                    builder.ResultResolvers.AddRange(resolvers);
                    builder.Assemblies.AddRange(assemblies!);
                    builder.Commands.AddRange(commands!);

                    return builder.Build();
                });

                services.Add(descriptor);
            });
        }
    }
}
