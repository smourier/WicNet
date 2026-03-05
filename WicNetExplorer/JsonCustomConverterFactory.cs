using System;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WicNetExplorer;

public class JsonCustomConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(Color);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(Color))
            return new JsonCustomColorConverter();

        throw new NotSupportedException();
    }

    private sealed class JsonCustomColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected string value for {nameof(Color)}.");

            var str = reader.GetString();
            if (string.IsNullOrEmpty(str) || !int.TryParse(str.TrimStart('#'), NumberStyles.HexNumber, null, out var argb))
                return Color.Transparent;

            return Color.FromArgb(argb);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"#{value.ToArgb():X8}");
        }
    }
}
