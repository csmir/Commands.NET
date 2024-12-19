﻿using Commands.Resolvers;
using System.Reflection;

namespace Commands.Builders
{
    /// <summary>
    ///     A configuration builder determining the build process for modules and commands in a <see cref="ComponentTree"/>. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     This builder is able to configure the following:
    ///     <list type="number">
    ///         <item>Defining assemblies through which will be searched to discover modules and commands.</item>
    ///         <item>Defining custom commands that do not appear in the assemblies.</item>
    ///         <item>Registering implementations of <see cref="ResultResolver"/> which define custom result handling.</item>
    ///         <item>Configuring settings for creation of modules and commands.</item>
    ///     </list>
    /// </remarks>
    public interface ITreeBuilder
    {
        /// <summary>
        ///     Gets or sets the configuration builder for the <see cref="ComponentTree"/>.
        /// </summary>
        public IConfigurationBuilder Configuration { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="CommandInfo"/>'s that are manually created before the registration process runs.
        /// </summary>
        public ICollection<IComponentBuilder> Components { get; set; }

        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        public ICollection<Assembly> Assemblies { get; set; }

        /// <summary>
        ///     Gets or sets a collection of <see cref="ResultResolver"/>'s that serve as post-execution handlers.
        /// </summary>
        public ICollection<ResultResolver> ResultResolvers { get; set; }

        /// <summary>
        ///     Gets or sets a filter that determines whether a created component should be yielded back to the registration process or skipped entirely, based on state provided within the component itself.
        /// </summary>
        /// <remarks>
        ///     Whether this filter returns true or false has no effect on the validation of component integrity, 
        ///     meaning that the build process will still fail if the component is not a valid command or module under its own restrictions.
        /// </remarks>
        public Func<IComponent, bool> ComponentRegistrationFilter { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether modules should be made read-only after the registration process.
        /// </summary>
        public bool MakeModulesReadonly { get; set; }

        /// <summary>
        ///     Gets or sets the naming convention regular expression which implementations of <see cref="IComponent"/> must adhere to be registered into the <see cref="ComponentTree"/>.
        /// </summary>
        public string? NamingPattern { get; set; }

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="commandBuilder">The builder instance to add to the collection, which will be built into a <see cref="CommandInfo"/> instance that can be executed by the <see cref="ComponentTree"/>.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(CommandBuilder commandBuilder);

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     When using this method, the command will be created with the default constructor. In order for the command to be valid for execution, <see cref="ModuleBuilder.WithAliases(string[])"/> must be called within <paramref name="configureCommand"/>.
        /// </remarks>
        /// <param name="configureCommand">An action that extends the fluent API of this type to configure the command.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(Action<CommandBuilder> configureCommand);

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="name">The name of the component.</param>
        /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(string name, Delegate executionDelegate);

        /// <summary>
        ///     Adds a command to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     Delegate based commands are able to access the command's scope by implementing <see cref="CommandContext{T}"/> as the first argument in the lambda signature.
        /// </remarks>
        /// <param name="name">The name of the component.</param>
        /// <param name="executionDelegate">The delegate to execute when the provided name of this object is discovered in a search operation.</param>
        /// <param name="aliases">The aliases of the component, excluding the name.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddCommand(string name, Delegate executionDelegate, params string[] aliases);

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <param name="moduleBuilder">The builder instance to add to the collection, which will be built into a <see cref="ModuleInfo"/> instance that can contain commands to be executed by the <see cref="ComponentTree"/>.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddModule(ModuleBuilder moduleBuilder);

        /// <summary>
        ///     Adds a module to the <see cref="Components"/> collection.
        /// </summary>
        /// <remarks>
        ///     When using this method, the module will be created with the default constructor. In order for the module to be valid for execution, <see cref="ModuleBuilder.WithAliases(string[])"/> must be called within <paramref name="configureModule"/>.
        /// </remarks>
        /// <param name="configureModule">An action that extends the fluent API of this type to configure the module.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddModule(Action<ModuleBuilder> configureModule);

        /// <summary>
        ///     Configures an action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="IExecuteResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="IExecuteResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddResultResolver(Action<ICallerContext, IExecuteResult, IServiceProvider> resultAction);

        /// <summary>
        ///     Configures an asynchronous action that runs when a command publishes its result. This action runs after all pipeline actions have been resolved.
        /// </summary>
        /// <remarks>
        ///     The <see cref="IExecuteResult"/> revealed by this action contains data about command success. 
        ///     Check <see cref="IExecuteResult.Success"/> to determine whether or not the command ran successfully.
        /// </remarks>
        /// <param name="resultAction">The action resembling a post-execution action based on the command result.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddResultResolver(Func<ICallerContext, IExecuteResult, IServiceProvider, ValueTask> resultAction);

        /// <summary>
        ///     Adds an implementation of <see cref="ResultResolver"/> to <see cref="ResultResolvers"/>.
        /// </summary>
        /// <param name="resolver">The implementation of <see cref="ResultResolver"/> to add.</param>
        /// <returns>The same <see cref="ITreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddResultResolver(ResultResolver resolver);

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional assembly, skipping the add operation if the assembly is already present.
        /// </summary>
        /// <param name="assembly">An assembly that should be added to <see cref="Assemblies"/>.</param>
        /// <returns>The same <see cref="ComponentTreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddAssembly(Assembly assembly);

        /// <summary>
        ///     Configures the <see cref="Assemblies"/> with an additional set of assemblies, skipping the add operation of an assembly in the collection if it is already present.
        /// </summary>
        /// <param name="assemblies">A collection of assemblies that should be added to <see cref="Assemblies"/>.</param>
        /// <returns>The same <see cref="ComponentTreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder AddAssemblies(params Assembly[] assemblies);

        /// <summary>
        ///     Configures the <see cref="ComponentRegistrationFilter"/> to filter components at registration, based on the provided predicate.
        /// </summary>
        /// <remarks>
        ///     <b>Whether this filter returns true or false has no effect on the validation of component integrity.</b>
        ///     This means that the build process will still fail if the component is not a valid command or module under its own restrictions.
        /// </remarks>
        /// <param name="filter">A predicate which determines if a component should be added to its parent component, or directly to the command tree if it is a top-level one.</param>
        /// <returns>The same <see cref="ComponentTreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder WithRegistrationFilter(Func<IComponent, bool> filter);

        /// <summary>
        ///     Configures the <see cref="Configuration"/> property with modified values through the <paramref name="configure"/> action.
        /// </summary>
        /// <param name="configure">An action that runs over the configuration to create .</param>
        /// <returns>The same <see cref="ComponentTreeBuilder"/> for call-chaining.</returns>
        public ITreeBuilder Configure(Action<IConfigurationBuilder> configure);

        /// <summary>
        ///     Builds the current <see cref="ITreeBuilder"/> instance into a new <see cref="ComponentTree"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="ComponentTree"/> built by this builder.</returns>
        public ComponentTree Build();
    }
}