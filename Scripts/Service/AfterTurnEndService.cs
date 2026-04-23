using AttackLog.Core;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;

namespace AttackLog.Service;

public class AfterTurnEndService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.AfterTurnEnd, OnAfterTurnEnd);
    }

    private static void OnAfterTurnEnd(IAttackLogEvent e)
    {
        var evt = (AfterTurnEndEvent)e;
        SaveTurnLog(evt.CombatState, evt.Side);
    }

    public static void SaveTurnLog(CombatState combatState, CombatSide side)
    {
        if (side == CombatSide.Player) return;

        if (LogState.Instance.RunLog == null) return;
        if (!LogState.Instance.RunLog.IsSameRun(combatState.RunState)) return;

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            LogState.Instance.TurnLogsData.TryGetValue(player.PlayerInfo, out var turnLogData);
            LogState.Instance.RoomLogsData.TryGetValue(player.PlayerInfo, out var roomLogData);

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
