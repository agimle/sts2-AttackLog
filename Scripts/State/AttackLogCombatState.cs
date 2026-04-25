using AttackLog.Model;

namespace AttackLog.State;

/// <summary>
/// 战斗级状态，管理单场战斗中的日志数据和怪物记录。
/// 战斗开始时清空 RoomLogsData 和 CombatRecord，战斗结束时清空所有数据。
/// </summary>
public class AttackLogCombatState
{
    /// <summary>当前战斗的怪物 Power 记录</summary>
    public CombatRecord CombatRecord { get; set; } = new();

    /// <summary>玩家 → 房间日志数据映射</summary>
    public Dictionary<PlayerInfo, SingleRoomLogData> RoomLogsData { get; set; } = new();

    /// <summary>玩家 → 回合日志数据映射</summary>
    public Dictionary<PlayerInfo, SingleTurnLogData> TurnLogsData { get; set; } = new();

    /// <summary>
    /// 战斗开始回调，清空房间日志和怪物记录
    /// </summary>
    public void OnCombatStart()
    {
        RoomLogsData.Clear();
        CombatRecord.Clear();
    }

    /// <summary>
    /// 战斗结束回调，清空回合日志和房间日志
    /// </summary>
    public void OnCombatEnd()
    {
        TurnLogsData.Clear();
        RoomLogsData.Clear();
    }
}
