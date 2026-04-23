using AttackLog.Core;
using AttackLog.Model;
using AttackLog.State;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;

namespace AttackLog.Service;

public static class BeforeSideTurnStartService
{
    public static void Subscribe()
    {
        AttackLogEventBus.Subscribe(AttackLogEventType.BeforeSideTurnStart, OnBeforeSideTurnStart);
    }

    private static void OnBeforeSideTurnStart(IAttackLogEvent e)
    {
        var evt = (BeforeSideTurnStartEvent)e;
        CreateNewTurnLog(evt.CombatState, evt.Side);
        UpdatePoisonPower(evt.CombatState, evt.Side);
    }

    public static void CreateNewTurnLog(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player) return;

        if (LogState.Instance.RunLog is null) return;
        if (!LogState.Instance.RunLog.IsSameRun(combatState.RunState)) return;

        LogState.Instance.TurnLogsData.Clear();

        foreach (var player in LogState.Instance.RunLog.GetAllPlayers())
        {
            SingleTurnLogData newTurnLogData = new SingleTurnLogData();
            LogState.Instance.TurnLogsData.TryAdd(player.PlayerInfo, newTurnLogData);
        }

        LogState.Instance.InvalidateCache();
    }

    public static void UpdatePoisonPower(CombatState combatState, CombatSide side)
    {
        if (side != CombatSide.Player)
        {
            return;
        }

        foreach (var monsterRecord in LogState.Instance.CombatRecord.MonsterRecords)
        {
            foreach (var powerRecord in monsterRecord.Value.Powers)
            {
                if (powerRecord.Key == typeof(PoisonPower))
                {
                    if (powerRecord.Value.Count == 0) continue;

                    powerRecord.Value.Peek().Amount--;

                    if (powerRecord.Value.Peek().Amount <= 0)
                    {
                        powerRecord.Value.Dequeue();
                    }
                }
            }
        }
    }
}
