using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

/// <summary>
/// 每次战斗结束后
/// </summary>
public static class AfterCombatEndService
{
    /// <summary>
    /// 保存该房间的战斗记录
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    public static void SaveRoomLog(IRunState? runState, CombatState? combatState)
    {
        if (runState is null || LogState.Instance.RunLog == null) return;
        if (!LogState.Instance.RunLog.IsSameRun(runState)) return;

        // 遍历每一个注册过的玩家
        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            // 读取模组保存的数据
            LogState.Instance.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);
            if (turnLogData != null && roomLogData != null)
            {
                roomLogData.EnqueueTurnLogData(turnLogData);
                roomLogData.RoomLogSum.Plus(turnLogData.TurnLogSum);
            }

            if (roomLogData != null)
            {
                // 存入对应玩家的RunLogData里
                player.RunLogData.EnqueueRoomLogData(roomLogData);
                player.RunLogData.RunLogSum.Plus(roomLogData.RoomLogSum);
            }
        }
        LogState.Instance.TurnLogsData.Clear();
        LogState.Instance.RoomLogsData.Clear();
        LogState.Instance.InvalidateCache();
    }

    /// <summary>
    /// 保存模组存档
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    public static void SaveLogState(IRunState? runState, CombatState? combatState)
    {
        ModSaveUtils.Save(runState, combatState);
    }
}