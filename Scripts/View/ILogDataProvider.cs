using AttackLog.Model;
using AttackLog.State;

namespace AttackLog.View;

public interface ILogDataProvider
{
    bool HasActiveRun { get; }
    IReadOnlyCollection<PlayerData> GetPlayers();
    CachedPlayerStats? GetPlayerStats(PlayerInfo info);
}
