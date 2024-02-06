using Commands.Helpers;
using Commands.TypeConverters;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Commands.Core
{
    /// <summary>
    ///     The configuration to be implemented by a new <see cref="CommandManager"/>.
    /// </summary>
    public class CommandConfiguration
    {
        private Assembly[] _assemblies = [Assembly.GetEntryAssembly()];

        /// <summary>
        ///     Gets a collection of assemblies that the <see cref="CommandManager"/> will use to register commands.
        /// </summary>
        /// <remarks>
        ///     If the collection has not been altered, it will include <see cref="Assembly.GetEntryAssembly"/> as the default value.
        /// </remarks>
        public Assembly[] Assemblies
        {
            get
            {
                return _assemblies;
            }
        }

        private TypeConverter[] _typeReaders = [];

        /// <summary>
        ///     Gets a collection of <see cref="TypeConverter"/>'s that the <see cref="CommandManager"/> will use to handle unknown argument types.
        /// </summary>
        /// <remarks>
        ///     It is adviced <b>not</b> to create new implementations of <see cref="TypeConverter"/> without first confirming if the target type is not already supported. 
        ///     All valuetypes and time/date types are already supported out of the box.
        /// </remarks>
        public TypeConverter[] Converters
        {
            get
            {
                return _typeReaders;
            }
        }

        private ResultResolver _resultResolver = ResultResolver.Default;

        /// <summary>
        ///     Gets or sets a resolver that contains a handler for handling command post-execution data.
        /// </summary>
        public ResultResolver ResultResolver
        {
            get
            {
                return _resultResolver;
            }
            set
            {
                if (value == null)
                {
                    ThrowHelpers.ThrowInvalidArgument(value);
                }

                _resultResolver = value;
            }
        }

        private AsyncApproach _asyncApproach = AsyncApproach.Default;

        /// <summary>
        ///     Gets or sets the approach to asynchronousity in commands.
        /// </summary>
        /// <remarks>
        ///     If set to <see cref="AsyncApproach.Await"/>, the manager will wait for a command to finish before allowing another to be executed.
        ///     If set to <see cref="AsyncApproach.Discard"/>, the manager will seperate the command execution from the entry stack, and slip it to another thread. 
        ///     Only change this value if you have read the documentation of <see cref="Core.AsyncApproach"/> and understand the definitions.
        /// </remarks>
        public AsyncApproach AsyncApproach
        {
            get
            {
                return _asyncApproach;
            }
            set
            {
                if (value is not AsyncApproach.Await or AsyncApproach.Discard)
                {
                    ThrowHelpers.ThrowInvalidOperation("AsyncApproach does not support values that exceed the provided options, ranging between 0 and 1.");
                }

                _asyncApproach = value;
            }
        }

        /// <summary>
        ///     Replaces the existing values in <see cref="Assemblies"/> with a new collection.
        /// </summary>
        /// <remarks>
        ///     To prevent duplicate value recognition, <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})"/> is called to remove duplicates from <paramref name="assemblies"/>.
        /// </remarks>
        /// <param name="assemblies">A collection of <see cref="Assembly"/> targets to be used to register commands.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration WithAssemblies(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assemblies);
            }

            _assemblies = assemblies.Distinct().ToArray();

            return this;
        }

        /// <summary>
        ///     Adds an <see cref="Assembly"/> to <see cref="Assemblies"/>.
        /// </summary>
        /// <remarks>
        ///     This call will throw if <see cref="Assemblies"/> already contains a value for <paramref name="assembly"/>.
        /// </remarks>
        /// <param name="assembly">An <see cref="Assembly"/> target to be used to register commands.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                ThrowHelpers.ThrowInvalidArgument(assembly);
            }

            if (_assemblies.Contains(assembly))
            {
                ThrowHelpers.NotDistinct(assembly);
            }

            AddAsm(assembly);

            return this;
        }

        /// <summary>
        ///     Attempts to add an <see cref="Assembly"/> to <see cref="Assemblies"/>.
        /// </summary>
        /// <remarks>
        ///     Will not add <paramref name="assembly"/> to <see cref="Assemblies"/> if it already contains the target assembly.
        /// </remarks>
        /// <param name="assembly">An <see cref="Assembly"/> target to be used to register commands.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration TryAddAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                if (!_assemblies.Contains(assembly))
                {
                    AddAsm(assembly);
                }
            }

            return this;
        }

        /// <summary>
        ///     Replaces the existing values in <see cref="Converters"/> with a new collection.
        /// </summary>
        /// <remarks>
        ///     To prevent duplicate value recognition, <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})"/> is called to remove duplicates from <paramref name="typeReaders"/>.
        /// </remarks>
        /// <param name="typeReaders">A collection of <see cref="TypeConverter"/>'s to parse unknown argument types.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration WithTypeReaders(params TypeConverter[] typeReaders)
        {
            if (typeReaders == null)
            {
                ThrowHelpers.ThrowInvalidArgument(typeReaders);
            }

            _typeReaders = typeReaders.Distinct(TypeConverter.EqualityComparer.Default).ToArray();

            return this;
        }

        /// <summary>
        ///     Adds a <see cref="TypeConverter"/> to <see cref="Converters"/>.
        /// </summary>
        /// <remarks>
        ///     This call will throw if <see cref="Converters"/> already contains an implementation of the target type, validated by a custom equality comparer.
        /// </remarks>
        /// <param name="typeReader">A <see cref="TypeConverter"/> to parse unknown argument types.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration AddTypeReader(TypeConverter typeReader)
        {
            if (typeReader == null)
            {
                ThrowHelpers.ThrowInvalidArgument(typeReader);
            }

            if (_typeReaders.Contains(typeReader, TypeConverter.EqualityComparer.Default))
            {
                ThrowHelpers.NotDistinct(typeReader);
            }

            AddTr(typeReader);

            return this;
        }

        /// <summary>
        ///     Attempts to add a <see cref="TypeConverter"/> to <see cref="Converters"/>.
        /// </summary>
        /// <remarks>
        ///     Will not add <paramref name="typeReader"/> to <see cref="Converters"/> if it already contains an implementation of the target type, validated by a custom equality comparer.
        /// </remarks>
        /// <param name="typeReader">A <see cref="TypeConverter"/> to parse unknown argument types.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration TryAddTypeReader(TypeConverter typeReader)
        {
            if (typeReader != null)
            {
                if (!_typeReaders.Contains(typeReader, TypeConverter.EqualityComparer.Default))
                {
                    AddTr(typeReader);
                }
            }

            return this;
        }

        /// <summary>
        ///     Sets the <see cref="ResultResolver.SuccessHandle"/> on the present <see cref="ResultResolver"/>, being <see cref="ResultResolver.Default"/> if not set.
        /// </summary>
        /// <param name="configureDelegate">The delegate that configures what action should be taken when the pipeline returns success.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration OnSuccess([DisallowNull] Func<ICommandContext, ICommandResult, IServiceProvider, Task> configureDelegate)
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            _resultResolver.FailHandle = configureDelegate;

            return this;
        }

        /// <summary>
        ///     Sets the <see cref="ResultResolver.SuccessHandle"/> on the present <see cref="ResultResolver"/>, being <see cref="ResultResolver.Default"/> if not set.
        /// </summary>
        /// <param name="configureDelegate">The delegate that configures what action should be taken when the pipeline returns failure.</param>
        /// <returns>The same <see cref="CommandConfiguration"/> for call chaining.</returns>
        public CommandConfiguration OnFailure([DisallowNull] Func<ICommandContext, ICommandResult, IServiceProvider, Task> configureDelegate)
        {
            if (configureDelegate == null)
            {
                ThrowHelpers.ThrowInvalidArgument(configureDelegate);
            }

            _resultResolver.SuccessHandle = configureDelegate;

            return this;
        }

        private void AddAsm(Assembly assembly)
        {
            var oLen = _assemblies.Length;
            Array.Resize(ref _assemblies, oLen + 1);

            _assemblies[oLen] = assembly;
        }

        private void AddTr(TypeConverter typeReader)
        {
            var oLen = _typeReaders.Length;
            Array.Resize(ref _typeReaders, oLen + 1);

            _typeReaders[oLen] = typeReader;
        }
    }
}
