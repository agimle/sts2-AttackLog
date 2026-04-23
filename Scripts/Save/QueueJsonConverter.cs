using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AttackLog.Save;

public class QueueJsonConverter<T> : JsonConverter<Queue<T>>
{
    public override Queue<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
        return list != null ? new Queue<T>(list) : new Queue<T>();
    }

    public override void Write(Utf8JsonWriter writer, Queue<T> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, new List<T>(value), options);
    }
}
