using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AttackLog.Save;

/// <summary>
/// Queue&lt;T&gt; 的 JSON 转换器，将 Queue 序列化为 List、反序列化时还原为 Queue。
/// 解决 System.Text.Json 不原生支持 Queue 的问题
/// </summary>
/// <typeparam name="T">队列元素类型</typeparam>
public class QueueJsonConverter<T> : JsonConverter<Queue<T>>
{
    /// <summary>
    /// 从 JSON 数组反序列化为 Queue
    /// </summary>
    public override Queue<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
        return list != null ? new Queue<T>(list) : new Queue<T>();
    }

    /// <summary>
    /// 将 Queue 序列化为 JSON 数组
    /// </summary>
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
