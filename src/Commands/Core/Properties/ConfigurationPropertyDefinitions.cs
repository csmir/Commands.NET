using System.ComponentModel;

namespace Commands
{
    /// <summary>
    ///     Defines the names of configuration properties that can be used to configure the behavior of the <see cref="ComponentTree"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ConfigurationPropertyDefinitions
    {
        /// <summary>
        ///     Gets the name of the configuration property that defines the validation expression for the name of a command or module.
        /// </summary>
        public const string NameValidationExpression = "NameValidationExpression";

        /// <summary>
        ///     Gets the name of the configuration property that defines the filter expression for creating a command or module in the <see cref="ComponentTree"/>.
        /// </summary>
        public const string ComponentRegistrationExpression = "ComponentRegistrationExpression";

        /// <summary>
        ///     Gets the name of the configuration property that defines the write expression used to validate the build log.
        /// </summary>
        public const string WriteBuildLogExpression = "BuildLogExpression";

        /// <summary>
        ///     Gets the name of the configuration property that defines if modules should be created to be read-only during the build process.
        /// </summary>
        public const string MakeModulesReadonly = "MakeModulesReadonly";
    }
}
