using AttackLog.Core;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace AttackLog.Service;

public static class BeforeCombatStartService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.BeforeCombatStart, OnBeforeCombatStart);
    }

    private static void OnBeforeCombatStart(IAttackLogEvent e)
    {
        var evt = (BeforeCombatStartEvent)e;
        CreateNewRoomLog(evt.RunState, evt.CombatState);
    }

    public static void CreateNewRoomLog(IRunState? runState, CombatState? combatState)
    {
        if (LogState.Instance.RunLog is null || runState is null)
        {
            return;
        }

        if (!LogState.Instance.RunLog.IsSameRun(runState))
        {
            return;
        }

        LogState.Instance.OnCombatStart();

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            SingleRoomLogData newRoomLogData = new SingleRoomLogData();
            LogState.Instance.RoomLogsData.TryAdd(player.PlayerInfo, newRoomLogData);
        }

        LogState.Instance.InvalidateCache();
    }
}
