using System.Text.Json;
using AttackLog.Model;
using Godot;

namespace AttackLog.Save;

public static class ModSaveConfig
{
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };
    
    public static readonly string SavePath = ProjectSettings.GlobalizePath("user://log_save.json");
    
    static ModSaveConfig()
    {
        JsonOptions.Converters.Add(new QueueJsonConverter<AttackLogModel>());
        JsonOptions.Converters.Add(new QueueJsonConverter<SingleTurnLogData>());
        JsonOptions.Converters.Add(new QueueJsonConverter<SingleRoomLogData>());
    }
}