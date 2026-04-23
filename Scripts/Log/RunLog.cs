using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Model;

public class RunLog
{
    /// <summary>
    /// 当前游戏
    /// </summary>
    private readonly IRunState _runState;
    /// <summary>
    /// 玩家Id -> 玩家数据
    /// </summary>
    private readonly Dictionary<ulong, PlayerData> _netIdToPlayers = new();
    /// <summary>
    /// 玩家生物 -> 玩家数据
    /// </summary>
    private readonly Dictionary<Creature, PlayerData> _creaturesToPlayers = new();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="runState"></param>
    public RunLog(IRunState runState)
    {
        _runState = runState;
    }

    /// <summary>
    /// 是否为同一场游戏
    /// </summary>
    /// <param name="runState"></param>
    /// <returns></returns>
    public bool IsSameRun(IRunState runState)
    {
        return _runState == runState;
    }

    /// <summary>
    /// 注册单个玩家
    /// </summary>
    /// <param name="player"></param>
    public void RegisterPlayer(Player player)
    {
        var playerData = new PlayerData(player);
        _netIdToPlayers.TryAdd(player.NetId, playerData);
        _creaturesToPlayers.TryAdd(player.Creature, playerData);
    }

    public PlayerData? GetPlayerByNetId(ulong netId)
    {
        return _netIdToPlayers.TryGetValue(netId, out var data) ? data : null;
    }

    public PlayerData? GetPlayerByCreature(Creature creature)
    {
        return _creaturesToPlayers.TryGetValue(creature, out var data) ? data : null;
    }

    public IReadOnlyCollection<PlayerData> GetAllPlayers()
    {
        return _netIdToPlayers.Values;
    }
}