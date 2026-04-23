using AttackLog.Core;
using AttackLog.Save;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

public static class AfterCombatEndService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.AfterCombatEnd, OnAfterCombatEnd);
    }

    private static void OnAfterCombatEnd(IAttackLogEvent e)
    {
        var evt = (AfterCombatEndEvent)e;
        SaveRoomLog(evt.RunState, evt.CombatState);
        SaveLogState(evt.RunState, evt.CombatState);
    }

    public static void SaveRoomLog(IRunState? runState, CombatState? combatState)
    {
        if (runState is null || LogState.Instance.RunLog == null) return;
        if (!LogState.Instance.RunLog.IsSameRun(runState)) return;

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            LogState.Instance.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);
            if (turnLogData != null && roomLogData != null)
            {
                roomLogData.EnqueueTurnLogData(turnLogData);
                roomLogData.RoomLogSum.Plus(turnLogData.TurnLogSum);
            }

            if (roomLogData != null)
            {
                player.RunLogData.EnqueueRoomLogData(roomLogData);
                player.RunLogData.RunLogSum.Plus(roomLogData.RoomLogSum);
            }
        }

        LogState.Instance.OnCombatEnd();
    }

    public static void SaveLogState(IRunState? runState, CombatState? combatState)
    {
        ModSaveUtils.Save(runState, combatState);
    }
}
