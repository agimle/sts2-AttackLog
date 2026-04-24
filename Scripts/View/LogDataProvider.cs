using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

public class LogDataProvider : ILogDataProvider
{
    public static readonly ILogDataProvider Default = new LogDataProvider();

    public bool HasActiveRun => LogState.Instance.RunLog != null;

    public IReadOnlyCollection<PlayerData> GetPlayers()
    {
        var runLog = LogState.Instance.RunLog;
        if (runLog == null) return Array.Empty<PlayerData>();
        return runLog.GetAllPlayers();
    }

    public CachedPlayerStats? GetPlayerStats(PlayerInfo info)
    {
        return LogState.Instance.GetCachedStats(info);
    }
}
