using Commands.Core;
using Commands.Reflection;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Commands.TypeConverters
{
    internal class ColorConverter : TypeConverter<Color>
    {
        private readonly Dictionary<string, Color> _colors;
        private readonly Dictionary<string, string> _spacedColors;

        public ColorConverter()
        {
            var properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var colors = new Dictionary<string, Color>(properties.Length - 1, StringComparer.OrdinalIgnoreCase);

            foreach (var property in properties)
                colors[property.Name] = (Color)property.GetValue(null);

            _colors = colors;

            var spacedNames = new Dictionary<string, string>(_colors.Comparer);
            var builder = new StringBuilder();
            foreach (var name in _colors.Keys)
            {
                builder.Clear();
                var hasSpace = false;
                for (var i = 0; i < name.Length; i++)
                {
                    var character = name[i];
                    if (i != 0 && char.IsUpper(character))
                    {
                        hasSpace = true;
                        builder.Append(' ');
                    }

                    builder.Append(character);
                }

                if (hasSpace)
                    spacedNames[builder.ToString()] = name;
            }

            _spacedColors = spacedNames;
        }

        public override ValueTask<ConvertResult> EvaluateAsync(ICommandContext context, IServiceProvider services, IArgument parameter, string value, CancellationToken cancellationToken)
        {
            if (int.TryParse(value.Replace("#", "").Replace("0x", ""), NumberStyles.HexNumber, null, out var hexNumber))
                return ValueTask.FromResult(Success(Color.FromArgb(hexNumber)));

            var name = value;

            _spacedColors.TryGetValue(name, out name);

            if (_colors.TryGetValue(name, out var color))
                return ValueTask.FromResult(Success(color));

            return ValueTask.FromResult(Error($"The provided value is not a color. Got: '{value}'. At: '{parameter.Name}'"));
        }
    }
}
