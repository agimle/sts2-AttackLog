using System.Text;
using Godot;

namespace AttackLog.Logger;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public static class ModLogger
{
    private static readonly string LogPath = ProjectSettings.GlobalizePath("user://attack_log_debug.txt");
    private static LogLevel _minLevel = LogLevel.Info;
    private static StreamWriter? _writer;
    private static readonly StringBuilder StringBuilder = new();

    static ModLogger()
    {
        try
        {
            _writer = new StreamWriter(LogPath, append: true) { AutoFlush = false };
        }
        catch { }
    }

    public static void Debug(string message) => Write(LogLevel.Debug, message);
    public static void Debug(string tag, string message) => Write(LogLevel.Debug, $"[{tag}] {message}");

    public static void Info(string message) => Write(LogLevel.Info, message);
    public static void Info(string tag, string message) => Write(LogLevel.Info, $"[{tag}] {message}");

    public static void Warning(string tag, string message) => Write(LogLevel.Warning, $"[{tag}] {message}");

    public static void Error(string tag, string message) => Write(LogLevel.Error, $"[{tag}] {message}");

    public static void Log(string message)
    {
        Write(LogLevel.Info, message);
    }

    public static void Log(string tag, string message)
    {
        Write(LogLevel.Info, $"[{tag}] {message}");
    }

    private static void Write(LogLevel level, string message)
    {
        if (level < _minLevel) return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string line = $"[{timestamp}][{level}] {message}";

        StringBuilder.AppendLine(line);

        try
        {
            _writer?.WriteLine(line);
            if (level >= LogLevel.Warning)
                _writer?.Flush();
        }
        catch { }
    }

    public static void Flush()
    {
        _writer?.Flush();
    }

    public static void Clear()
    {
        StringBuilder.Clear();
        _writer?.Flush();
        _writer?.Close();
        try
        {
            _writer = new StreamWriter(LogPath, append: false) { AutoFlush = false };
        }
        catch { }
    }
}
