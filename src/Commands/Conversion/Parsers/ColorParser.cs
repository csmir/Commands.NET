using System.Drawing;
using System.Globalization;

namespace Commands;

/// <summary>
///     A parser that converts a string value to a <see cref="Color"/>.
/// </summary>
public sealed class ColorParser : TypeParser<Color>
{
    private readonly Dictionary<string, Color> _colors;

    /// <summary>
    ///     Initializes a new instance of <see cref="ColorParser"/>.
    /// </summary>
    public ColorParser()
    {
        var writer = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

        var properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(Color))
            {
                var color = (Color)property.GetValue(null)!;
                writer[property.Name] = color;
            }
        }

        _colors = writer;
    }

    /// <inheritdoc />
    public override ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        // The order of operations is built to be as efficient as possible, from the most common input types taking priority.
        if (value is not string str)
            str = value?.ToString() ?? string.Empty;

        // Named colors, being the most commonly used to describe colors.
        // Takes: Red, Green, Blue etc.
        if (TryParseNamed(str, out var color))
            return Success(color);

        // Hex color codes, being most commonly used color identification.
        // Takes: #FFFFFF or FFFFFF
        if (TryParseHex(str, out color))
            return Success(color);

        // RGB color codes, being second most common and used in many programming languages as well as editors.
        // Takes: 000,000,000 or 000 000 000 or 000, 000, 000
        if (TryParseRgb(str, out color))
            return Success(color);

        // UInt32 color codes, being the least common but still valid.
        // Takes: 0x00000000 or 0xFFFFFFFF
        if (TryParseUint(str, out color))
            return Success(color);

        return Error($"The provided value: {str} is not a valid color.");
    }

    private bool TryParseRgb(string value, out Color result)
    {
        result = new();

        var separation = value.Split(',');

        if (separation.Length == 3)
        {
            if (byte.TryParse(separation[0], out var r) &&
                byte.TryParse(separation[1], out var g) &&
                byte.TryParse(separation[2], out var b))
            {
                result = Color.FromArgb(r, g, b);
                return true;
            }
        }

        return false;
    }

    private bool TryParseHex(string value, out Color result)
    {
        result = new();

#if NET8_0_OR_GREATER
        if (value.StartsWith('#'))
            value = value[1..];
#else
        if (value.StartsWith("#"))
            value = value.Substring(1);
#endif

        if (value.Length == 6)
        {
            if (uint.TryParse(value, NumberStyles.HexNumber, null, out var rgb))
            {
                var r = (byte)((rgb & 0xFF0000) >> 16);
                var g = (byte)((rgb & 0x00FF00) >> 8);
                var b = (byte)(rgb & 0x0000FF);
                result = Color.FromArgb(r, g, b);
                return true;
            }
        }

        return false;
    }

    private bool TryParseUint(string value, out Color result)
    {
        result = new();

        if (uint.TryParse(value, out var rgb))
        {
            var r = (byte)((rgb & 0xFF0000) >> 16);
            var g = (byte)((rgb & 0x00FF00) >> 8);
            var b = (byte)(rgb & 0x0000FF);

            result = Color.FromArgb(r, g, b);

            return true;
        }

        return false;
    }

    private bool TryParseNamed(string value, out Color result)
    {
        result = new();

        if (_colors.TryGetValue(value, out var color))
        {
            result = color;

            return true;
        }

        return false;
    }
}
