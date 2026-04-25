using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

/// <summary>
/// 默认日志数据提供者，从 LogState 单例获取数据。
/// 可通过 AttackLogPanel.SetDataProvider 替换为自定义实现
/// </summary>
public class LogDataProvider : ILogDataProvider
{
    /// <summary>默认单例实例</summary>
    public static readonly ILogDataProvider Default = new LogDataProvider();

    /// <summary>是否存在活跃的 Run</summary>
    public bool HasActiveRun => LogState.Instance.RunLog != null;

    /// <summary>
    /// 获取所有已注册的玩家数据
    /// </summary>
    public IReadOnlyCollection<PlayerData> GetPlayers()
    {
        var runLog = LogState.Instance.RunLog;
        if (runLog == null) return Array.Empty<PlayerData>();
        return runLog.GetAllPlayers();
    }

    /// <summary>
    /// 获取指定玩家的缓存统计数据
    /// </summary>
    /// <param name="info">玩家信息</param>
    /// <returns>缓存统计，不存在返回 null</returns>
    public CachedPlayerStats? GetPlayerStats(PlayerInfo info)
    {
        return LogState.Instance.GetCachedStats(info);
    }
}
