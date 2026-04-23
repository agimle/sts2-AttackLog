using System.Text.Json;
using Godot;

namespace AttackLog.Save;

public static class SaveConfig
{
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };
    
    public static readonly string SavePath = ProjectSettings.GlobalizePath("user://log_save.json");
}