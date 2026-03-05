using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using WicNetExplorer.Model;

namespace WicNetExplorer;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [
        typeof(JsonCustomConverterFactory),
        typeof(JsonStringEnumConverter<InterpolationMode>),
        ])]
[JsonSerializable(typeof(Settings))]
[JsonSerializable(typeof(Color))]
[JsonSerializable(typeof(RecentFile))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(object[]))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(long[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(ulong[]))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(uint[]))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(ushort[]))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(short[]))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(double[]))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(float[]))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(byte[]))]
internal partial class JsonSourceGenerationContext : JsonSerializerContext
{
}