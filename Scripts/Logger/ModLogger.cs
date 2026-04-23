using System.Text;
using Godot;

namespace AttackLog.Logger;

public static class ModLogger
{
    private static readonly string LogPath = ProjectSettings.GlobalizePath("user://attack_log_debug.txt");
    private static readonly StringBuilder StringBuilder = new();

    public static void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string line = $"[{timestamp}] {message}";
        
        StringBuilder.AppendLine(line);
        
        File.AppendAllText(LogPath, line + "\n");
    }

    public static void Log(string tag, string message)
    {
        Log($"[{tag}] {message}");
    }

    public static void Clear()
    {
        StringBuilder.Clear();
        if (File.Exists(LogPath))
        {
            File.Delete(LogPath);
        }
    }
}
