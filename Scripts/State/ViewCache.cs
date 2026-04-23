using AttackLog.Model;

namespace AttackLog.State;

public class ViewCache
{
    private Dictionary<PlayerInfo, CachedPlayerStats> _statsCache = new();
    private bool _cacheValid = false;

    public void Invalidate()
    {
        _cacheValid = false;
    }

    public CachedPlayerStats? GetStats(PlayerInfo info)
    {
        if (!_cacheValid)
        {
            RebuildCache();
        }
        return _statsCache.TryGetValue(info, out var stats) ? stats : null;
    }

    public Dictionary<PlayerInfo, CachedPlayerStats> GetAllStats()
    {
        if (!_cacheValid)
        {
            RebuildCache();
        }
        return _statsCache;
    }

    private void RebuildCache()
    {
        _statsCache.Clear();

        var runLog = LogState.Instance.RunLog;
        if (runLog == null)
        {
            _cacheValid = true;
            return;
        }

        var players = runLog.GetAllPlayers();
        if (players.Count == 0)
        {
            _cacheValid = true;
            return;
        }

        int totalDamage = 0;

        foreach (var player in players)
        {
            var stats = CalculatePlayerStats(player.PlayerInfo);
            _statsCache[player.PlayerInfo] = stats;
            totalDamage += stats.RunLog?.RealDamageDealt ?? 0;
        }

        foreach (var player in players)
        {
            if (_statsCache.TryGetValue(player.PlayerInfo, out var stats))
            {
                int playerDamage = stats.RunLog?.RealDamageDealt ?? 0;
                stats.DamagePercent = totalDamage > 0
                    ? (float)playerDamage / totalDamage * 100f
                    : 0f;
                stats.TotalDamage = totalDamage;
            }
        }

        _cacheValid = true;
    }

    private CachedPlayerStats CalculatePlayerStats(PlayerInfo info)
    {
        var stats = new CachedPlayerStats();

        stats.TurnLog = GetPlayerTurnLog(info);
        stats.RoomLog = GetPlayerRoomLog(info);
        stats.RunLog = GetPlayerRunLog(info);

        return stats;
    }

    private AttackLogModel? GetPlayerTurnLog(PlayerInfo playerInfo)
    {
        if (LogState.Instance.RunLog == null) return null;

        return LogState.Instance.TurnLogsData.TryGetValue(playerInfo, out var turnLogData)
            ? turnLogData.TurnLogSum
            : new AttackLogModel();
    }

    private AttackLogModel? GetPlayerRoomLog(PlayerInfo playerInfo)
    {
        if (LogState.Instance.RunLog == null) return null;

        AttackLogModel? turnLog = GetPlayerTurnLog(playerInfo);
        if (turnLog == null) turnLog = new AttackLogModel();

        return LogState.Instance.RoomLogsData.TryGetValue(playerInfo, out var roomLogData)
            ? AttackLogModel.Sum(roomLogData.RoomLogSum, turnLog)
            : new AttackLogModel();
    }

    private AttackLogModel? GetPlayerRunLog(PlayerInfo playerInfo)
    {
        if (LogState.Instance.RunLog == null) return null;

        AttackLogModel? roomLog = GetPlayerRoomLog(playerInfo);
        if (roomLog == null) roomLog = new AttackLogModel();

        PlayerData? playerData = LogState.Instance.RunLog.GetPlayerByCreature(playerInfo.Creature);
        if (playerData == null) return new AttackLogModel();

        return AttackLogModel.Sum(playerData.RunLogData.RunLogSum, roomLog);
    }
}
