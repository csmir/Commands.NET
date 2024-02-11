using Commands.Conditions;
using Commands.Core;
using Commands.Helpers;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command module, hosting zero-or-more commands.
    /// </summary>
    public sealed class ModuleInfo : IConditional
    {
        /// <summary>
        ///     Gets an array containing nested modules or commands inside this module.
        /// </summary>
        public IReadOnlySet<IConditional> Components { get; }

        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc />
        public string? Name { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public bool IsQueryable { get; }

        /// <inheritdoc />
        public bool IsDefault { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public PreconditionAttribute[] Preconditions { get; }

        /// <inheritdoc />
        public PostconditionAttribute[] PostConditions { get; }

        /// <inheritdoc />
        public byte Priority { get; }

        /// <inheritdoc />
        public ModuleInfo? Module { get; }

        /// <inheritdoc />
        public float Score
        {
            get
            {
                return GetScore();
            }
        }

        internal ModuleInfo(
            Type type, ModuleInfo? root, string[] aliases, BuildOptions options)
        {
            Priority = 0;

            var attributes = type.GetAttributes(true);
            var preconditions = attributes.GetPreconditions();
            var postconditions = attributes.GetPostconditions();

            Module = root;
            Type = type;

            Attributes = attributes;
            Preconditions = preconditions;
            PostConditions = postconditions;

            Components = this.GetComponents(aliases.Length > 0, options)
                .OrderByDescending(x => x.Score)
                .ToHashSet();

            Aliases = aliases;

            if (aliases.Length > 0)
            {
                IsQueryable = true;
                Name = aliases[0];
            }
            else
            {
                IsQueryable = false;
                Name = null;
            }

            IsDefault = true;
        }

        /// <inheritdoc />
        public float GetScore()
        {
            if (Components.Count == 0)
                return 0.0f;

            var score = 1.0f;

            foreach (var component in Components)
            {
                score += component.GetScore();
            }

            if (Name != Type.Name)
                score += 1.0f;

            score += Priority;

            return score;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{(Module != null ? $"{Module}." : "")}{(Name != null ? $"{Type.Name}['{Name}']" : $"{Type.Name}")}";
        }
    }
}
