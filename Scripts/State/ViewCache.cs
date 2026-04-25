using AttackLog.Model;

namespace AttackLog.State;

public class ViewCache
{
    private readonly Func<RunLog?> _runLogProvider;
    private readonly Func<IReadOnlyDictionary<PlayerInfo, SingleTurnLogData>> _turnLogsProvider;
    private readonly Func<IReadOnlyDictionary<PlayerInfo, SingleRoomLogData>> _roomLogsProvider;

    private Dictionary<PlayerInfo, CachedPlayerStats> _statsCache = new();
    private bool _cacheValid = false;

    public ViewCache(
        Func<RunLog?> runLogProvider,
        Func<IReadOnlyDictionary<PlayerInfo, SingleTurnLogData>> turnLogsProvider,
        Func<IReadOnlyDictionary<PlayerInfo, SingleRoomLogData>> roomLogsProvider)
    {
        _runLogProvider = runLogProvider;
        _turnLogsProvider = turnLogsProvider;
        _roomLogsProvider = roomLogsProvider;
    }

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
        var runLog = _runLogProvider();
        if (runLog == null)
        {
            _statsCache.Clear();
            _cacheValid = true;
            return;
        }

        var players = runLog.GetAllPlayers();
        if (players.Count == 0)
        {
            _statsCache.Clear();
            _cacheValid = true;
            return;
        }

        var currentPlayerInfos = new HashSet<PlayerInfo>();
        int totalDamage = 0;

        foreach (var player in players)
        {
            currentPlayerInfos.Add(player.PlayerInfo);

            if (!_statsCache.TryGetValue(player.PlayerInfo, out var stats))
            {
                stats = new CachedPlayerStats();
                _statsCache[player.PlayerInfo] = stats;
            }

            UpdatePlayerStats(stats, player.PlayerInfo);
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

        var toRemove = _statsCache.Keys
            .Where(k => !currentPlayerInfos.Contains(k))
            .ToList();
        foreach (var key in toRemove)
        {
            _statsCache.Remove(key);
        }

        _cacheValid = true;
    }

    private void UpdatePlayerStats(CachedPlayerStats stats, PlayerInfo info)
    {
        stats.TurnLog = GetPlayerTurnLog(info);
        stats.RoomLog = GetPlayerRoomLog(info);
        stats.RunLog = GetPlayerRunLog(info);
    }

    private AttackLogModel? GetPlayerTurnLog(PlayerInfo playerInfo)
    {
        var runLog = _runLogProvider();
        if (runLog == null) return null;

        return _turnLogsProvider().TryGetValue(playerInfo, out var turnLogData)
            ? turnLogData.TurnLogSum
            : new AttackLogModel();
    }

    private AttackLogModel? GetPlayerRoomLog(PlayerInfo playerInfo)
    {
        var runLog = _runLogProvider();
        if (runLog == null) return null;

        AttackLogModel? turnLog = GetPlayerTurnLog(playerInfo);
        if (turnLog == null) turnLog = new AttackLogModel();

        return _roomLogsProvider().TryGetValue(playerInfo, out var roomLogData)
            ? AttackLogModel.Sum(roomLogData.RoomLogSum, turnLog)
            : new AttackLogModel();
    }

    private AttackLogModel? GetPlayerRunLog(PlayerInfo playerInfo)
    {
        var runLog = _runLogProvider();
        if (runLog == null) return null;

        AttackLogModel? roomLog = GetPlayerRoomLog(playerInfo);
        if (roomLog == null) roomLog = new AttackLogModel();

        PlayerData? playerData = runLog.GetPlayerByCreature(playerInfo.Creature);
        if (playerData == null) return new AttackLogModel();

        return AttackLogModel.Sum(playerData.RunLogData.RunLogSum, roomLog);
    }
}
