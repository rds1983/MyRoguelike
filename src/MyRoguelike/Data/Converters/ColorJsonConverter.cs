using System.Text.Json;
using System.Text.Json.Serialization;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace MyRoguelike.Data.Converters;

public class ColorJsonConverter : JsonConverter<MonoGameColor>
{
    public override MonoGameColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int r = 0, g = 0, b = 0, a = 255;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new MonoGameColor(r, g, b, a);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var prop = reader.GetString()?.ToLowerInvariant();
                reader.Read();
                var value = reader.GetInt32();

                switch (prop)
                {
                    case "r": r = value; break;
                    case "g": g = value; break;
                    case "b": b = value; break;
                    case "a": a = value; break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, MonoGameColor value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("r", value.R);
        writer.WriteNumber("g", value.G);
        writer.WriteNumber("b", value.B);
        writer.WriteNumber("a", value.A);
        writer.WriteEndObject();
    }
}
