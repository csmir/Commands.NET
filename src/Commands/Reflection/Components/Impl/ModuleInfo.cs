using Commands.Conditions;
using System.Collections;
using System.Diagnostics;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command module, hosting zero-or-more commands.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class ModuleInfo : ISearchable, IEnumerable<ISearchable>
    {
        private readonly HashSet<ISearchable> _components;

        /// <summary>
        ///     Gets an array containing nested modules or commands inside this module.
        /// </summary>
        public IReadOnlyCollection<ISearchable> Components
            => _components;

        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type? Type { get; }

        /// <inheritdoc />
        public IInvoker? Invoker { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] PreEvaluations { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] PostEvaluations { get; }

        /// <inheritdoc />
        public float Priority { get; }

        /// <inheritdoc />
        public ModuleInfo? Module { get; }

        /// <inheritdoc />
        public string? Name
            => Aliases.Length > 0 ? Aliases[0] : null;

        /// <inheritdoc />
        public string FullName
            => $"{(Module != null && Module.Name != null ? $"{Module.FullName} " : "")}{Name}";

        /// <inheritdoc />
        public float Score
            => GetScore();

        /// <inheritdoc />
        public bool IsRuntimeComponent
            => Type == null;

        /// <inheritdoc />
        public bool IsSearchable
            => Aliases.Length > 0;

        /// <inheritdoc />
        public bool IsDefault
            => false;

        /// <summary>
        ///     Gets the number of components in the module.
        /// </summary>
        public int Count
            => _components.Count;

        /// <summary>
        ///     Gets the depth of the module, being how deeply nested it is in the command tree.
        /// </summary>
        public int Depth
            => Module?.Depth + 1 ?? 0;

        internal ModuleInfo(
            Type type, ModuleInfo? root, string[] aliases, CommandConfiguration options)
        {
            Module = root;
            Type = type;

            var attributes = type.GetAttributes(true).Concat(root?.Attributes ?? []).Distinct();

            Attributes = attributes.ToArray();

            PreEvaluations = ConditionEvaluator.CreateEvaluators(attributes.OfType<IPreExecutionCondition>()).ToArray();
            PostEvaluations = ConditionEvaluator.CreateEvaluators(attributes.OfType<IPostExecutionCondition>()).ToArray();

            Priority = attributes.GetAttribute<PriorityAttribute>()?.Priority ?? 0;

            Aliases = aliases;

            Invoker = new ConstructorInvoker(type);

            _components = [.. ReflectionUtilities.GetComponents(this, options).OrderByDescending(x => x.Score)];
        }

        internal ModuleInfo(
            ModuleInfo? root, string[] aliases)
        {
            Module = root;

            _components = [];
            Attributes = [];
            PreEvaluations = [];
            PostEvaluations = [];

            Aliases = aliases;
        }

        /// <inheritdoc />
        public float GetScore()
        {
            if (Components.Count == 0)
                return 0.0f;

            var score = 1.0f;

            foreach (var component in Components)
                score += component.GetScore();

            if (Name != Type?.Name)
                score += 1.0f;

            score += Priority;

            return score;
        }

        /// <summary>
        ///     Adds a component to the current module.
        /// </summary>
        /// <param name="component">The component to be added to the module.</param>
        /// <returns><see langword="true"/> if the component was added; otherwise, <see langword="false"/>.</returns>
        public bool AddComponent(ISearchable component)
            => AddComponents(component) > 0;

        /// <summary>
        ///     Adds all provided components to the current module.
        /// </summary>
        /// <param name="components">The components to be added to the module.</param>
        /// <returns>The number of added components, being 0 if no records were added.</returns>
        public int AddComponents(params ISearchable[] components)
        {
            var hasChanged = 0;

            foreach (var component in components)
                hasChanged += (_components.Add(component) ? 1 : 0);

            var copy = _components.OrderByDescending(x => x.Score);

            _components.Clear();

            foreach (var copiedItem in copy)
                _components.Add(copiedItem);

            return hasChanged;
        }

        /// <summary>
        ///     Removes a component from the current module if it exists.
        /// </summary>
        /// <param name="component">The component to be added to the module.</param>
        /// <returns><see langword="true"/> if the component was removed; otherwise, <see langword="false"/>.</returns>
        public bool RemoveComponent(ISearchable component)
            => _components.Remove(component);

        /// <summary>
        ///     Removes all components from the current module that match the predicate.
        /// </summary>
        /// <param name="predicate">The predicate which determines which items should be removed from the module.</param>
        /// <returns>The number of removed components, being 0 if no records were removed.</returns>
        public int RemoveComponents(Predicate<ISearchable> predicate)
            => _components.RemoveWhere(predicate);

        /// <summary>
        ///    Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="HashSet{T}.Enumerator"/> object for the underlying <see cref="HashSet{T}"/> of this module.</returns>
        public IEnumerator<ISearchable> GetEnumerator()
            => _components.GetEnumerator();

        /// <inheritdoc />
        public override string ToString()
            => $"{(Module != null ? $"{Module}." : "")}{(Name != null ? $"{Type?.Name}['{Name}']" : $"{Type?.Name}")}";

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
