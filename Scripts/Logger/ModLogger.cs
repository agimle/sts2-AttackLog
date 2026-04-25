using System.Text;
using Godot;

namespace AttackLog.Logger;

/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    /// <summary>调试</summary>
    Debug,
    /// <summary>信息</summary>
    Info,
    /// <summary>警告</summary>
    Warning,
    /// <summary>错误</summary>
    Error
}

/// <summary>
/// Mod 日志工具，将日志同时写入内存 StringBuilder 和磁盘文件。
/// 支持 Debug/Info/Warning/Error 四个级别，Warning 及以上立即刷盘
/// </summary>
public static class ModLogger
{
    /// <summary>日志文件路径</summary>
    private static readonly string LogPath = ProjectSettings.GlobalizePath("user://logs/attack_log_debug.txt");

    /// <summary>最低输出级别</summary>
    private static LogLevel _minLevel = LogLevel.Info;

    /// <summary>磁盘写入器</summary>
    private static StreamWriter? _writer;

    /// <summary>内存日志缓冲</summary>
    private static readonly StringBuilder StringBuilder = new();

    static ModLogger()
    {
        try
        {
            _writer = new StreamWriter(LogPath, append: true) { AutoFlush = false };
        }
        catch { }
    }

    /// <summary>输出 Debug 级别日志</summary>
    public static void Debug(string message) => Write(LogLevel.Debug, message);

    /// <summary>输出带标签的 Debug 级别日志</summary>
    public static void Debug(string tag, string message) => Write(LogLevel.Debug, $"[{tag}] {message}");

    /// <summary>输出 Info 级别日志</summary>
    public static void Info(string message) => Write(LogLevel.Info, message);

    /// <summary>输出带标签的 Info 级别日志</summary>
    public static void Info(string tag, string message) => Write(LogLevel.Info, $"[{tag}] {message}");

    /// <summary>输出带标签的 Warning 级别日志</summary>
    public static void Warning(string tag, string message) => Write(LogLevel.Warning, $"[{tag}] {message}");

    /// <summary>输出带标签的 Error 级别日志</summary>
    public static void Error(string tag, string message) => Write(LogLevel.Error, $"[{tag}] {message}");

    /// <summary>输出 Info 级别日志（Log 别名）</summary>
    public static void Log(string message)
    {
        Write(LogLevel.Info, message);
    }

    /// <summary>输出带标签的 Info 级别日志（Log 别名）</summary>
    public static void Log(string tag, string message)
    {
        Write(LogLevel.Info, $"[{tag}] {message}");
    }

    /// <summary>
    /// 核心写入方法：低于最低级别则跳过，Warning 及以上立即刷盘
    /// </summary>
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

    /// <summary>手动刷盘</summary>
    public static void Flush()
    {
        _writer?.Flush();
    }

    /// <summary>清空日志文件和内存缓冲，重新创建写入器</summary>
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
