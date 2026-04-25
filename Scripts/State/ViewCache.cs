using AttackLog.Model;

namespace AttackLog.State;

/// <summary>
/// 视图缓存，延迟计算并缓存玩家的统计数据。
/// 通过 Func 委托从 LogState 获取数据源，实现与 LogState 的解耦。
/// 缓存失效后首次访问时自动重建，避免频繁的重复计算。
/// </summary>
public class ViewCache
{
    /// <summary>RunLog 数据源委托</summary>
    private readonly Func<RunLog?> _runLogProvider;

    /// <summary>回合日志数据源委托</summary>
    private readonly Func<IReadOnlyDictionary<PlayerInfo, SingleTurnLogData>> _turnLogsProvider;

    /// <summary>房间日志数据源委托</summary>
    private readonly Func<IReadOnlyDictionary<PlayerInfo, SingleRoomLogData>> _roomLogsProvider;

    /// <summary>玩家统计缓存</summary>
    private Dictionary<PlayerInfo, CachedPlayerStats> _statsCache = new();

    /// <summary>缓存是否有效</summary>
    private bool _cacheValid = false;

    /// <summary>
    /// 构造视图缓存，通过 Func 委托注入数据源
    /// </summary>
    /// <param name="runLogProvider">RunLog 数据源</param>
    /// <param name="turnLogsProvider">回合日志数据源</param>
    /// <param name="roomLogsProvider">房间日志数据源</param>
    public ViewCache(
        Func<RunLog?> runLogProvider,
        Func<IReadOnlyDictionary<PlayerInfo, SingleTurnLogData>> turnLogsProvider,
        Func<IReadOnlyDictionary<PlayerInfo, SingleRoomLogData>> roomLogsProvider)
    {
        _runLogProvider = runLogProvider;
        _turnLogsProvider = turnLogsProvider;
        _roomLogsProvider = roomLogsProvider;
    }

    /// <summary>
    /// 使缓存失效，下次访问时自动重建
    /// </summary>
    public void Invalidate()
    {
        _cacheValid = false;
    }

    /// <summary>
    /// 获取指定玩家的缓存统计，缓存失效时自动重建
    /// </summary>
    /// <param name="info">玩家信息</param>
    /// <returns>缓存统计，不存在返回 null</returns>
    public CachedPlayerStats? GetStats(PlayerInfo info)
    {
        if (!_cacheValid)
        {
            RebuildCache();
        }
        return _statsCache.TryGetValue(info, out var stats) ? stats : null;
    }

    /// <summary>
    /// 获取所有玩家的缓存统计，缓存失效时自动重建
    /// </summary>
    /// <returns>玩家信息 → 缓存统计的映射</returns>
    public Dictionary<PlayerInfo, CachedPlayerStats> GetAllStats()
    {
        if (!_cacheValid)
        {
            RebuildCache();
        }
        return _statsCache;
    }

    /// <summary>
    /// 重建缓存：遍历所有玩家，计算各维度的统计数据和伤害占比
    /// </summary>
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

    /// <summary>
    /// 更新单个玩家的统计数据
    /// </summary>
    private void UpdatePlayerStats(CachedPlayerStats stats, PlayerInfo info)
    {
        stats.TurnLog = GetPlayerTurnLog(info);
        stats.RoomLog = GetPlayerRoomLog(info);
        stats.RunLog = GetPlayerRunLog(info);
    }

    /// <summary>
    /// 获取玩家回合级统计
    /// </summary>
    private AttackLogModel? GetPlayerTurnLog(PlayerInfo playerInfo)
    {
        var runLog = _runLogProvider();
        if (runLog == null) return null;

        return _turnLogsProvider().TryGetValue(playerInfo, out var turnLogData)
            ? turnLogData.TurnLogSum
            : new AttackLogModel();
    }

    /// <summary>
    /// 获取玩家房间级统计（房间汇总 + 当前回合）
    /// </summary>
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

    /// <summary>
    /// 获取玩家 Run 级统计（Run 汇总 + 当前房间）
    /// </summary>
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
