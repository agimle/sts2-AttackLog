using System.Text.Json;
using AttackLog.Model;
using Godot;

namespace AttackLog.Save;

/// <summary>
/// 存档配置，定义 JSON 序列化选项和存档文件路径
/// </summary>
public static class ModSaveConfig
{
    /// <summary>JSON 序列化选项：缩进输出、包含字段、注册 Queue 转换器</summary>
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };
    
    /// <summary>存档文件路径（Godot user:// 目录下）</summary>
    public static readonly string SavePath = ProjectSettings.GlobalizePath("user://log_save.json");
    
    static ModSaveConfig()
    {
        JsonOptions.Converters.Add(new QueueJsonConverter<AttackLogModel>());
        JsonOptions.Converters.Add(new QueueJsonConverter<SingleTurnLogData>());
        JsonOptions.Converters.Add(new QueueJsonConverter<SingleRoomLogData>());
    }
}
