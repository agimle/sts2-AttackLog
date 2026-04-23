using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

/// <summary>
/// 每场战斗开始前
/// </summary>
public static class BeforeCombatStartService
{
    /// <summary>
    /// 创建新的房间记录
    /// </summary>
    /// <param name="runState"></param>
    /// <param name="combatState"></param>
    public static void CreateNewRoomLog(IRunState? runState, CombatState? combatState)
    {
        #region 检查

        if(LogState.Instance.RunLog is null || runState is null)
        {
            return;
        }
        
        if(!LogState.Instance.RunLog.IsSameRun(runState))
        {
            return;
        }

        #endregion
        
        // 清空上一个房间的静态记录
        LogState.Instance.RoomLogsData.Clear();
        
        // 清理上一个房间的战斗power记录
        LogState.Instance.CombatRecord.Clear();
        
        // 为每一个玩家创建新的记录
        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            SingleRoomLogData newRoomLogData = new SingleRoomLogData();
            LogState.Instance.RoomLogsData.TryAdd(player.PlayerInfo, newRoomLogData);
        }
        
        LogState.Instance.InvalidateCache();
    }
}