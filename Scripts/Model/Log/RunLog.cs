using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Model;

/// <summary>
/// Run 级日志，管理玩家注册与查找。
/// 使用双索引（NetId + Creature）实现 O(1) 查找。
/// </summary>
public class RunLog
{
    /// <summary>绑定的 RunState，用于判断是否为同一次 Run</summary>
    private readonly IRunState _runState;

    /// <summary>NetId → PlayerData 索引</summary>
    private readonly Dictionary<ulong, PlayerData> _netIdToPlayers = new();

    /// <summary>Creature → PlayerData 反向索引，用于 O(1) 按 Creature 查找玩家</summary>
    private readonly Dictionary<Creature, PlayerData> _creatureToPlayers = new();

    public RunLog(IRunState runState)
    {
        _runState = runState;
    }

    /// <summary>
    /// 判断当前 RunLog 是否属于同一次 Run
    /// </summary>
    /// <param name="runState">要比较的 RunState</param>
    /// <returns>是否为同一次 Run</returns>
    public bool IsSameRun(IRunState runState)
    {
        return _runState == runState;
    }

    /// <summary>
    /// 注册玩家，同时维护 NetId 和 Creature 两个索引
    /// </summary>
    /// <param name="player">游戏原生 Player 对象</param>
    public void RegisterPlayer(Player player)
    {
        var playerData = new PlayerData(player);
        _netIdToPlayers.TryAdd(player.NetId, playerData);
        _creatureToPlayers.TryAdd(player.Creature, playerData);
    }

    /// <summary>
    /// 按 NetId 查找玩家数据，O(1) 复杂度
    /// </summary>
    /// <param name="netId">玩家 NetId</param>
    /// <returns>玩家数据，不存在返回 null</returns>
    public PlayerData? GetPlayerByNetId(ulong netId)
    {
        return _netIdToPlayers.TryGetValue(netId, out var data) ? data : null;
    }

    /// <summary>
    /// 按 Creature 查找玩家数据，O(1) 复杂度。
    /// 高频调用场景（每次伤害事件），使用 Dictionary 反向索引替代线性遍历。
    /// </summary>
    /// <param name="creature">玩家 Creature 实例</param>
    /// <returns>玩家数据，不存在返回 null</returns>
    public PlayerData? GetPlayerByCreature(Creature creature)
    {
        return _creatureToPlayers.TryGetValue(creature, out var data) ? data : null;
    }

    /// <summary>
    /// 获取所有已注册的玩家数据
    /// </summary>
    /// <returns>玩家数据的只读集合</returns>
    public IReadOnlyCollection<PlayerData> GetAllPlayers()
    {
        return _netIdToPlayers.Values;
    }
}
