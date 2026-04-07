using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankProductsRegistry.Frontend.Utilities;

/// <summary>
/// Deserializa propiedades expuestas como string aunque la API envíe número (p. ej. enums sin JsonStringEnumConverter).
/// </summary>
public sealed class JsonStringOrNumberConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number => reader.TryGetInt64(out var n)
                ? n.ToString(CultureInfo.InvariantCulture)
                : reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            JsonTokenType.Null => string.Empty,
            _ => throw new JsonException($"Token inesperado {reader.TokenType} al leer string.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
