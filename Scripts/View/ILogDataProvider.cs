using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

/// <summary>
/// 日志数据提供者接口，解耦 Panel 与 LogState 的直接依赖。
/// 支持替换为自定义实现（如测试 Mock）
/// </summary>
public interface ILogDataProvider
{
    /// <summary>是否存在活跃的 Run</summary>
    bool HasActiveRun { get; }

    /// <summary>
    /// 获取所有已注册的玩家数据
    /// </summary>
    IReadOnlyCollection<PlayerData> GetPlayers();

    /// <summary>
    /// 获取指定玩家的缓存统计数据
    /// </summary>
    /// <param name="info">玩家信息</param>
    /// <returns>缓存统计，不存在返回 null</returns>
    CachedPlayerStats? GetPlayerStats(PlayerInfo info);
}
