using Commands.Conditions;
using Commands.Parsing;
using System.Collections;
using System.Diagnostics;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command module, hosting zero-or-more commands.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class ModuleInfo : ISearchable, ISearchableSet
    {
        private HashSet<ISearchable> _components;
        private readonly Action<ISearchable[]>? _notifyTopLevelMutation;

        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        ///     Gets the depth of the module, being how deeply nested it is in the command tree.
        /// </summary>
        public int Depth
            => Module?.Depth + 1 ?? 1;

        /// <inheritdoc />
        public int Count
            => _components.Count;

        /// <inheritdoc />
        public bool IsReadOnly { get; }

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

        internal ModuleInfo(
            Type type, ModuleInfo? root, string[] aliases, BuildConfiguration options)
        {
            // If the root is null, we are at the top-level module. In this case, we need to set a notifier for the command manager that its command tree has changed.
            // This is an expensive operation, so we only do it once for every collection of added components. See: AddComponent, AddComponents.
            if (root == null)
                _notifyTopLevelMutation = options.N_NotifyTopLevelMutation;

            IsReadOnly = options.SealModuleDefinitions;

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
            if (_components.Count == 0)
                return 0.0f;

            var score = 1.0f;

            foreach (var component in _components)
                score += component.GetScore();

            if (Name != Type?.Name)
                score += 1.0f;

            score += Priority;

            return score;
        }

        /// <inheritdoc />
        public IEnumerable<SearchResult> Find(ArgumentEnumerator args)
        {
            List<SearchResult> discovered = [];

            var searchHeight = Depth;

            foreach (var component in _components)
            {
                if (component.IsDefault)
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight));

                if (args.Length == searchHeight)
                    continue;

                if (!args.TryNext(searchHeight, out var value))
                    continue;

                if (!component.Aliases.Any(x => x == value))
                    continue;

                if (component is ModuleInfo module)
                {
                    var nested = module.Find(args);

                    discovered.AddRange(nested);
                    discovered.Add(SearchResult.FromError(module));
                }
                else
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetCommands(bool browseNestedComponents = true)
            => GetCommands(_ => true, browseNestedComponents);

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetCommands(Predicate<CommandInfo> predicate, bool browseNestedComponents = true)
        {
            if (!browseNestedComponents)
                return _components.Where(x => x is CommandInfo info && predicate(info));

            List<ISearchable> discovered = [];

            foreach (var component in _components)
            {
                if (component is CommandInfo command && predicate(command))
                    discovered.Add(command);

                if (component is ModuleInfo module)
                    discovered.AddRange(module.GetCommands(predicate, browseNestedComponents));
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetModules(bool browseNestedComponents = true)
            => GetModules(_ => true, browseNestedComponents);

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetModules(Predicate<ModuleInfo> predicate, bool browseNestedComponents = true)
        {
            if (!browseNestedComponents)
                return _components.Where(x => x is ModuleInfo info && predicate(info));

            List<ISearchable> discovered = [];

            foreach (var component in _components)
            {
                if (component is ModuleInfo module)
                {
                    if (predicate(module))
                        discovered.Add(component);

                    discovered.AddRange(module.GetModules(predicate, browseNestedComponents));
                }
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetAll()
            => _components;

        /// <inheritdoc />
        public int CountAll()
        {
            var sum = 0;
            foreach (var component in _components)
            {
                if (component is ModuleInfo module)
                    sum += module.CountAll();

                sum++;
            }

            return sum;
        }

        /// <inheritdoc />
        public bool Add(ISearchable component)
            => AddRange(component) > 0;

        /// <inheritdoc />
        public int AddRange(params ISearchable[] components)
        {
            if (IsReadOnly)
                throw ComponentException.AccessDenied();

            var hasChanged = 0;

            var copy = new HashSet<ISearchable>(_components);

            foreach (var component in components)
                hasChanged += (copy.Add(component) ? 1 : 0);

            if (hasChanged > 0)
            {
                var orderedCopy = new HashSet<ISearchable>(copy.OrderByDescending(x => x.Score));

                Interlocked.Exchange(ref _components, orderedCopy);

                _notifyTopLevelMutation?.Invoke(components);
            }

            return hasChanged;
        }

        /// <inheritdoc />
        public bool Remove(ISearchable component)
            => _components.Remove(component);

        /// <inheritdoc />
        public int RemoveWhere(Predicate<ISearchable> predicate)
        {
            if (IsReadOnly)
                throw ComponentException.AccessDenied();

            return _components.RemoveWhere(predicate);
        }

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
