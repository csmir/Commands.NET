using Commands.Converters;
using System.Text.RegularExpressions;

namespace Commands
{
    public interface IConfigurationBuilder
    {
        public Regex NamingRegex { get; set; }

        public List<TypeConverterBase> TypeConverters { get; set; }
    }
}
