using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.Service;

/// <summary>
/// 回合结束后
/// </summary>
public class AfterTurnEndService
{
    /// <summary>
    /// 保存回合记录
    /// </summary>
    /// <param name="combatState"></param>
    /// <param name="side"></param>
    public static void SaveTurnLog(CombatState combatState, CombatSide side)
    {
        if(side == CombatSide.Player) return;
        
        if (LogState.Instance.RunLog == null) return;
        if (!LogState.Instance.RunLog.IsSameRun(combatState.RunState)) return;

        // 遍历所有玩家
        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            // 获取模组保存的回合记录和房间记录
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            LogState.Instance.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);
            
            // 把回合记录存入房间记录
            if (turnLogData != null)
            {
                roomLogData?.EnqueueTurnLogData(turnLogData);
                roomLogData?.RoomLogSum.Plus(turnLogData.TurnLogSum);
            }
        }
        
        LogState.Instance.TurnLogsData.Clear();
        LogState.Instance.InvalidateCache();
    }
}